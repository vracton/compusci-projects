#include "Visualizer3DCore.h"
#include "Frustrum.h"
#include "directxmath.h"
#include "VisualizerCommand.h"

//#define MYDEBUG

Visualizer3DCore::Visualizer3DCore(HINSTANCE hInstance) :
	D3DApp(hInstance)
{

}

Visualizer3DCore::~Visualizer3DCore()
{
	if (getD3DDevice() != nullptr)
		flushCommandQueue();
}

bool Visualizer3DCore::initialize()
{
	while (meshes.empty() || mMaterials.empty() || objects.empty());
	while (getChangesNeeded());
	setDoneDrawing(false);
	if (!D3DApp::initialize())
		return false;

	// Reset the command list to prep for initialization commands.
	D3DUtil::throwIfFailed(getCommandList()->Reset(getCommandAllocator().Get(), nullptr));

	// Get the increment size of a descriptor in this heap type.  This is hardware specific, 
	// so we have to query this information.
	mCbvSrvDescriptorSize = getD3DDevice()->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

	BuildRootSignature();
	BuildShadersAndInputLayout();
	BuildShapeGeometry();

	autoCameraAdjust();

	BuildMaterials();
	BuildRenderItems();
	BuildFrameResources();
	BuildPSOs();

	//camera.SetPosition(0.0f, 2.0f, -15.0f);
	//camera.lookAt(XMFLOAT3(1.0f, 1.0f, 0.0f), XMFLOAT3(0.0f, 0.0f, 0.0f), XMFLOAT3(0.0f, 0.0f, 1.0f));

	//// Execute the initialization commands.
	D3DUtil::throwIfFailed(getCommandList()->Close());

	ID3D12CommandList* cmdsLists[] = { getCommandList().Get() };
	getCommandQueue()->ExecuteCommandLists(_countof(cmdsLists), cmdsLists);

	//// Wait until initialization is complete.
	flushCommandQueue();
	//MessageBox(nullptr, L"LALALA", L"HR Failed", MB_OK);

	alreadyInitialized = true;
	setDoneDrawing(true);

	return true;
}

void Visualizer3DCore::ReInitialize()
{
	//std::lock_guard<std::mutex> lock(mutex);
	//onResize();
	// Reset the command list to prep for initialization commands.
	D3DUtil::throwIfFailed(getCommandList()->Reset(getCommandAllocator().Get(), nullptr));

	//BuildRootSignature();
	//BuildShadersAndInputLayout();
	BuildShapeGeometry();

	BuildMaterials();
	BuildRenderItems();
	BuildFrameResources();
	//BuildPSOs();

	//// Execute the initialization commands.
	D3DUtil::throwIfFailed(getCommandList()->Close());
	ID3D12CommandList* cmdsLists[] = { getCommandList().Get() };
	getCommandQueue()->ExecuteCommandLists(_countof(cmdsLists), cmdsLists);

	//// Wait until initialization is complete.
	flushCommandQueue();
	//MessageBox(nullptr, L"LALALA", L"HR Failed", MB_OK);
}

void Visualizer3DCore::onResize()
{
	D3DApp::onResize();

	camera.setLens(0.25f * MathHelper::Pi, aspectRatio(), 1.0f, 1000.0f);
}

void Visualizer3DCore::update(const GameTimer& gt)
{
	OnKeyboardInput(gt);
	if (!meshesNeedUpdating)
	{
		UpdateCamera(gt);
	}

	// Cycle through the circular frame resource array.
	mCurrFrameResourceIndex = (mCurrFrameResourceIndex + 1) % D3DUtil::getNFrameResources();
	mCurrFrameResource = mFrameResources[mCurrFrameResourceIndex].get();

	// Has the GPU finished processing the commands of the current frame resource?
	// If not, wait until the GPU has completed commands up to this fence point.
	if (mCurrFrameResource->Fence != 0 && getFence()->GetCompletedValue() < mCurrFrameResource->Fence)
	{
		HANDLE eventHandle = CreateEventEx(nullptr, nullptr, false, EVENT_ALL_ACCESS);
		D3DUtil::throwIfFailed(getFence()->SetEventOnCompletion(mCurrFrameResource->Fence, eventHandle));
		WaitForSingleObject(eventHandle, INFINITE);
		CloseHandle(eventHandle);
	}

	AnimateMaterials(gt);
	UpdateObjectCBs(gt);
	UpdateMaterialCBs(gt);
	UpdateMainPassCB(gt);

	currentState = checkForNextState();
}

void Visualizer3DCore::draw(const GameTimer& gt)
{
	auto cmdListAlloc = mCurrFrameResource->CmdListAlloc;

	// Reuse the memory associated with command recording.
	// We can only reset when the associated command lists have finished execution on the GPU.
	D3DUtil::throwIfFailed(cmdListAlloc->Reset());

	// A command list can be reset after it has been added to the command queue via ExecuteCommandList.
	// Reusing the command list reuses memory.
	D3DUtil::throwIfFailed(getCommandList()->Reset(cmdListAlloc.Get(), mOpaquePSO.Get()));

	getCommandList()->RSSetViewports(1, &getScreenViewport());
	getCommandList()->RSSetScissorRects(1, &getScissorRect());

	// Indicate a state transition on the resource usage.
	auto transition = CD3DX12_RESOURCE_BARRIER::Transition(currentBackBuffer(),
		D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATE_RENDER_TARGET);
	getCommandList()->ResourceBarrier(1, &transition);

	// Clear the back buffer and depth buffer.
	getCommandList()->ClearRenderTargetView(currentBackBufferView(), Colors::Black, 0, nullptr);
	getCommandList()->ClearDepthStencilView(depthStencilView(), D3D12_CLEAR_FLAG_DEPTH | D3D12_CLEAR_FLAG_STENCIL, 1.0f, 0, 0, nullptr);

	// Specify the buffers we are going to render to.
	auto backBuffer = currentBackBufferView();
	auto depthStencil = depthStencilView();
	getCommandList()->OMSetRenderTargets(1, &backBuffer, true, &depthStencil);

	//ID3D12DescriptorHeap* descriptorHeaps[] = { mSrvDescriptorHeap.Get() };
	//getCommandList()->SetDescriptorHeaps(_countof(descriptorHeaps), descriptorHeaps);

	getCommandList()->SetGraphicsRootSignature(mRootSignature.Get());

	auto passCB = mCurrFrameResource->PassCB->Resource();
	getCommandList()->SetGraphicsRootConstantBufferView(2, passCB->GetGPUVirtualAddress());

	DrawRenderItems(getCommandList().Get(), mOpaqueItems);

	//getCommandList()->SetPipelineState(mPSOs["alphaTested"].Get());
	//DrawRenderItems(getCommandList().Get(), mRitemLayer[(int)RenderLayer::AlphaTested]);

	getCommandList()->SetPipelineState(mTransparentPSO.Get());
	DrawRenderItems(getCommandList().Get(), mTransparentItems);

	// Indicate a state transition on the resource usage.
	transition = CD3DX12_RESOURCE_BARRIER::Transition(currentBackBuffer(),
		D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATE_PRESENT);
	getCommandList()->ResourceBarrier(1, &transition);

	// Done recording commands.
	D3DUtil::throwIfFailed(getCommandList()->Close());

	// Add the command list to the queue for execution.
	ID3D12CommandList* cmdsLists[] = { getCommandList().Get() };
	getCommandQueue()->ExecuteCommandLists(_countof(cmdsLists), cmdsLists);

	// Swap the back and front buffers
	D3DUtil::throwIfFailed(getSwapChain()->Present(0, 0));
	setCurrentBackBufferIndex((getCurrentBackBufferIndex() + 1) % getSwapChainBufferCount());

	// Advance the fence value to mark commands up to this fence point.
	incrementCurrentFence();
	mCurrFrameResource->Fence = getCurrentFence();

	// Add an instruction to the command queue to set a new fence point. 
	// Because we are on the GPU timeline, the new fence point won't be 
	// set until the GPU finishes processing all the commands prior to this Signal().
	getCommandQueue()->Signal(getFence().Get(), getCurrentFence());

	currentState = State::ReadyToUpdate;
}

void Visualizer3DCore::runStateMachine(const GameTimer& gt)
{
	currentlyExecuting = true;

	switch (currentState)
	{
	case State::Initializing:
		InitializeDirectX();
		break;

	case State::ReadyToBuildMeshes:
		ProcessCommandQueue();
		if (!meshes.empty() && !mMaterials.empty())
		{
			BuildMeshes();
		}
		break;

	case State::ReadyToBuildObjects:
		ProcessCommandQueue();
		if (!objects.empty())
		{
			BuildObjects();
		}
		break;

	case State::Paused:
		ProcessCommandQueue();
		if (!isPaused())
		{
			Unpause();
		}
		break;

	case State::ReadyToUpdate:
		ProcessCommandQueue();
		update(gt);
		break;

	case State::ReadyToDraw:
		draw(gt);
		break;
	}

	currentlyExecuting = false;
}

void Visualizer3DCore::onMouseDown(WPARAM btnState, int x, int y)
{
	mLastMousePos.x = x;
	mLastMousePos.y = y;

	SetCapture(mainWindow());
}

void Visualizer3DCore::onMouseUp(WPARAM btnState, int x, int y)
{
	ReleaseCapture();
}

void Visualizer3DCore::onMouseMove(WPARAM btnState, int x, int y)
{
	if ((btnState & MK_LBUTTON) != 0)
	{
		// Make each pixel correspond to a quarter of a degree.
		float dx = XMConvertToRadians(0.1f * static_cast<float>(x - mLastMousePos.x));
		float dy = XMConvertToRadians(0.1f * static_cast<float>(y - mLastMousePos.y));

		camera.pitch(dy);
		camera.yaw(dx);
	}
	else if ((btnState & MK_RBUTTON) != 0)
	{
		// Make each pixel correspond to 0.2 unit in the scene.
		float dx = 0.05f * static_cast<float>(x - mLastMousePos.x);
		float dy = 0.05f * static_cast<float>(y - mLastMousePos.y);

		// Update the camera radius based on input.
		camera.walk(dx - dy);
	}

	mLastMousePos.x = x;
	mLastMousePos.y = y;
}

void Visualizer3DCore::OnKeyboardInput(const GameTimer& gt)
{
	const float dt = gt.DeltaTime();

	if (GetAsyncKeyState('W') & 0x8000)
	{
		//MessageBox(nullptr, L"LALALA", L"HR Failed", MB_OK);
		cameraVel.x += cameraAccel * dt;
		MathHelper::Clamp(cameraVel.x, -maxSpeed, maxSpeed);
	}
	else if (GetAsyncKeyState('S') & 0x8000)
	{
		cameraVel.x -= cameraAccel * dt;
		MathHelper::Clamp(cameraVel.x, -maxSpeed, maxSpeed);
	}
	else if (cameraVel.x > 0)
	{
		cameraVel.x -= cameraDrag * dt;
		if (cameraVel.x < 0)
			cameraVel.x = 0;
	}
	else if (cameraVel.x < 0)
	{
		cameraVel.x += cameraDrag * dt;
		if (cameraVel.x > 0)
			cameraVel.x = 0;
	}

	if (GetAsyncKeyState('A') & 0x8000)
	{
		cameraVel.y -= cameraAccel * dt;
		MathHelper::Clamp(cameraVel.y, -maxSpeed, maxSpeed);
	}
	else if (GetAsyncKeyState('D') & 0x8000)
	{
		cameraVel.y += cameraAccel * dt;
		MathHelper::Clamp(cameraVel.y, -maxSpeed, maxSpeed);
	}
	else if (cameraVel.y < 0)
	{
		cameraVel.y += cameraDrag * dt;
		if (cameraVel.y > 0)
			cameraVel.y = 0;
	}
	else if (cameraVel.y > 0)
	{
		cameraVel.y -= cameraDrag * dt;
		if (cameraVel.y < 0)
			cameraVel.y = 0;
	}

	if (GetAsyncKeyState(VK_LSHIFT) & 0x8000)
	{
		cameraVel.z += cameraAccel * dt;
		MathHelper::Clamp(cameraVel.z, -maxSpeed, maxSpeed);
	}
	else if (GetAsyncKeyState(VK_SPACE) & 0x8000)
	{
		cameraVel.z -= cameraAccel * dt;
		MathHelper::Clamp(cameraVel.z, -maxSpeed, maxSpeed);
	}
	else if (cameraVel.z > 0)
	{
		cameraVel.z -= cameraDrag * dt;
		if (cameraVel.z < 0)
			cameraVel.z = 0;
	}
	else if (cameraVel.z < 0)
	{
		cameraVel.z += cameraDrag * dt;
		if (cameraVel.z > 0)
			cameraVel.z = 0;
	}

	if (GetAsyncKeyState(VK_UP) & 0x8000)
	{
		camera.pitch(-cameraRotation);
	}

	if (GetAsyncKeyState(VK_DOWN) & 0x8000)
	{
		camera.pitch(cameraRotation);
	}

	if (GetAsyncKeyState(VK_LEFT) & 0x8000)
	{
		camera.yaw(-cameraRotation);
	}

	if (GetAsyncKeyState(VK_RIGHT) & 0x8000)
	{
		camera.yaw(cameraRotation);
	}

	if (GetAsyncKeyState('Q') & 0x8000)
	{
		camera.roll(cameraRotation);
	}

	if (GetAsyncKeyState('E') & 0x8000)
	{
		camera.roll(-cameraRotation);
	}
}

void Visualizer3DCore::UpdateCamera(const GameTimer& gt)
{
	const float dt = gt.DeltaTime();

	if (cameraVel.x != 0)
	{
		camera.walk(cameraVel.x * dt);
	}
	if (cameraVel.y != 0)
	{
		camera.strafe(cameraVel.y * dt);
	}
	if (cameraVel.z != 0)
	{
		camera.elevate(cameraVel.z * dt);
	}

	rescaleZ();

	if (autoCamera)
	{
		autoCameraAdjust();
	}
}

void Visualizer3DCore::AnimateMaterials(const GameTimer& gt)
{

}

void Visualizer3DCore::UpdateObjectCBs(const GameTimer& gt)
{
	auto currObjectCB = mCurrFrameResource->ObjectCB.get();
	//std::lock_guard<std::mutex> lock(mutex);
	for (auto& e : mAllRitems)
	{
		// Only update the cbuffer data if the constants have changed.  
		// This needs to be tracked per frame resource.
		if (e->NumFramesDirty > 0)
		{
			//MessageBox(nullptr, L"LALALA", L"HR Failed", MB_OK);
			XMMATRIX world = XMLoadFloat4x4(&e->World);
			XMMATRIX texTransform = XMLoadFloat4x4(&e->TexTransform);

			ObjectConstants objConstants;
			XMStoreFloat4x4(&objConstants.World, XMMatrixTranspose(world));
			XMStoreFloat4x4(&objConstants.TexTransform, XMMatrixTranspose(texTransform));

			currObjectCB->CopyData(e->ObjCBIndex, objConstants);

			// Next FrameResource need to be updated too.
			e->NumFramesDirty--;
		}
	}
}

void Visualizer3DCore::UpdateMaterialCBs(const GameTimer& gt)
{
	auto currMaterialCB = mCurrFrameResource->MaterialCB.get();
	for (auto& e : mMaterials)
	{
		// Only update the cbuffer data if the constants have changed.  If the cbuffer
		// data changes, it needs to be updated for each FrameResource.
		Material* mat = e.get();
		if (mat->NumFramesDirty > 0)
		{
			XMMATRIX matTransform = XMLoadFloat4x4(&mat->MatTransform);

			MaterialConstants matConstants;
			matConstants.DiffuseAlbedo = mat->DiffuseAlbedo;
			matConstants.FresnelR0 = mat->FresnelR0;
			matConstants.Roughness = mat->Roughness;
			XMStoreFloat4x4(&matConstants.MatTransform, XMMatrixTranspose(matTransform));

			currMaterialCB->CopyData(mat->MatCBIndex, matConstants);

			// Next FrameResource need to be updated too.
			mat->NumFramesDirty--;
		}
	}
}

void Visualizer3DCore::UpdateMainPassCB(const GameTimer& gt)
{
	XMMATRIX view = camera.getView();
	XMMATRIX proj = camera.getProj();

	XMMATRIX viewProj = XMMatrixMultiply(view, proj);
	auto determinant = XMMatrixDeterminant(view);
	XMMATRIX invView = XMMatrixInverse(&determinant, view);
	determinant = XMMatrixDeterminant(proj);
	XMMATRIX invProj = XMMatrixInverse(&determinant, proj);
	determinant = XMMatrixDeterminant(viewProj);
	XMMATRIX invViewProj = XMMatrixInverse(&determinant, viewProj);

	XMStoreFloat4x4(&mMainPassCB.View, XMMatrixTranspose(view));
	XMStoreFloat4x4(&mMainPassCB.InvView, XMMatrixTranspose(invView));
	XMStoreFloat4x4(&mMainPassCB.Proj, XMMatrixTranspose(proj));
	XMStoreFloat4x4(&mMainPassCB.InvProj, XMMatrixTranspose(invProj));
	XMStoreFloat4x4(&mMainPassCB.ViewProj, XMMatrixTranspose(viewProj));
	XMStoreFloat4x4(&mMainPassCB.InvViewProj, XMMatrixTranspose(invViewProj));
	mMainPassCB.EyePosW = camera.getPosition3f();
	mMainPassCB.RenderTargetSize = XMFLOAT2(static_cast<float>(getClientWidth()), static_cast<float>(getClientHeight()));
	mMainPassCB.InvRenderTargetSize = XMFLOAT2(1.0f / getClientWidth(), 1.0f / getClientHeight());
	mMainPassCB.NearZ = camera.getNearZ();
	mMainPassCB.FarZ = camera.getFarZ();
	mMainPassCB.TotalTime = gt.TotalTime();
	mMainPassCB.DeltaTime = gt.DeltaTime();
	mMainPassCB.AmbientLight = { 0.25f, 0.25f, 0.35f, 1.0f };
	mMainPassCB.Lights[0].Direction = { 0.57735f, -0.57735f, 0.57735f };
	mMainPassCB.Lights[0].Strength = { 0.6f, 0.6f, 0.6f };
	mMainPassCB.Lights[1].Direction = { -0.57735f, -0.57735f, 0.57735f };
	mMainPassCB.Lights[1].Strength = { 0.3f, 0.3f, 0.3f };
	mMainPassCB.Lights[2].Direction = { 0.0f, -0.707f, -0.707f };
	mMainPassCB.Lights[2].Strength = { 0.15f, 0.15f, 0.15f };

	auto currPassCB = mCurrFrameResource->PassCB.get();
	currPassCB->CopyData(0, mMainPassCB);
}

void Visualizer3DCore::BuildRootSignature()
{
	// Root parameter can be a table, root descriptor or root constants.
	CD3DX12_ROOT_PARAMETER slotRootParameter[3];

	// Perfomance TIP: Order from most frequent to least frequent.
	slotRootParameter[0].InitAsConstantBufferView(0);
	slotRootParameter[1].InitAsConstantBufferView(1);
	slotRootParameter[2].InitAsConstantBufferView(2);

	//auto staticSamplers = GetStaticSamplers();

	// A root signature is an array of root parameters.
	CD3DX12_ROOT_SIGNATURE_DESC rootSigDesc(3, slotRootParameter, 0, nullptr, D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT);

	// create a root signature with a single slot which points to a descriptor range consisting of a single constant buffer
	ComPtr<ID3DBlob> serializedRootSig = nullptr;
	ComPtr<ID3DBlob> errorBlob = nullptr;
	HRESULT hr = D3D12SerializeRootSignature(&rootSigDesc, D3D_ROOT_SIGNATURE_VERSION_1,
		serializedRootSig.GetAddressOf(), errorBlob.GetAddressOf());

	if (errorBlob != nullptr)
	{
		::OutputDebugStringA((char*)errorBlob->GetBufferPointer());
	}
	D3DUtil::throwIfFailed(hr);

	D3DUtil::throwIfFailed(getD3DDevice()->CreateRootSignature(
		0,
		serializedRootSig->GetBufferPointer(),
		serializedRootSig->GetBufferSize(),
		IID_PPV_ARGS(mRootSignature.GetAddressOf())));
}

void Visualizer3DCore::BuildShadersAndInputLayout()
{
	//const D3D_SHADER_MACRO alphaTestDefines[] =
	//{
	//	"ALPHA_TEST", "1",
	//	NULL, NULL
	//};

#ifdef MYDEBUG
	mShaders["standardVS"] = D3DUtil::compileShader(L"Shaders\\Default.hlsl", nullptr, "VS", "vs_5_1");
	mShaders["opaquePS"] = D3DUtil::compileShader(L"Shaders\\Default.hlsl", nullptr, "PS", "ps_5_1");
	//mShaders["alphaTestedPS"] = D3DUtil::compileShader(L"..\\..\\..\\..\\VisualizerControl\\Shaders\\Default.hlsl", alphaTestDefines, "PS", "ps_5_0");
#else
	mShaders["standardVS"] = D3DUtil::compileShader(L"..\\..\\..\\..\\VisualizerControl\\Shaders\\Default.hlsl", nullptr, "VS", "vs_5_1");
	mShaders["opaquePS"] = D3DUtil::compileShader(L"..\\..\\..\\..\\VisualizerControl\\Shaders\\Default.hlsl", nullptr, "PS", "ps_5_1");
	//mShaders["alphaTestedPS"] = D3DUtil::compileShader(L"..\\..\\..\\..\\VisualizerControl\\Shaders\\Default.hlsl", alphaTestDefines, "PS", "ps_5_0");
#endif
	mInputLayout =
	{
		{ "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
		{ "NORMAL", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
		//{ "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 24, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
	};
}

void Visualizer3DCore::BuildShapeGeometry()
{
	std::vector<UINT> vertexOffsets;
	UINT runningSum = 0;
	for (const auto& mesh : meshes)
	{
		vertexOffsets.push_back(runningSum);
		runningSum += static_cast<UINT>(mesh.Vertices.size());
	}

	std::vector<UINT> indexOffsets;
	runningSum = 0;
	for (const auto& mesh : meshes)
	{
		indexOffsets.push_back(runningSum);
		runningSum += static_cast<UINT>(mesh.Indices32.size());
	}

	std::vector<SubmeshGeometry> submeshes;
	int counter = 0;
	for (const auto& mesh : meshes)
	{
		SubmeshGeometry subGeo;
		subGeo.IndexCount = static_cast<UINT>(mesh.Indices32.size());
		subGeo.StartIndexLocation = indexOffsets[counter];
		subGeo.BaseVertexLocation = vertexOffsets[counter];
		submeshes.push_back(subGeo);
		++counter;
	}

	std::vector<Vertex> vertices;
	storedVertices.clear();

	for (const auto& mesh : meshes)
	{
		std::vector<Vertex> stored;
		for (const auto& vertex : mesh.Vertices)
		{
			Vertex newVertex;
			newVertex.Pos = vertex.Position;
			newVertex.Normal = vertex.Normal;
			vertices.push_back(newVertex);

			stored.push_back(newVertex);
		}
		storedVertices.push_back(stored);
	}

	std::vector<std::uint16_t> indices;

	for (const auto& mesh : meshes)
	{
		auto& indices16 = mesh.GetIndices16();
		indices.insert(indices.end(), std::begin(indices16), std::end(indices16));
	}

	const UINT vbByteSize = (UINT)vertices.size() * sizeof(Vertex);
	const UINT ibByteSize = (UINT)indices.size() * sizeof(std::uint16_t);

	auto geo = std::make_unique<MeshGeometry>();
	geo->Name = "shapeGeo";

	D3DUtil::throwIfFailed(D3DCreateBlob(vbByteSize, &geo->VertexBufferCPU));
	CopyMemory(geo->VertexBufferCPU->GetBufferPointer(), vertices.data(), vbByteSize);

	D3DUtil::throwIfFailed(D3DCreateBlob(ibByteSize, &geo->IndexBufferCPU));
	CopyMemory(geo->IndexBufferCPU->GetBufferPointer(), indices.data(), ibByteSize);

	geo->VertexBufferGPU = D3DUtil::createDefaultBuffer(getD3DDevice().Get(),
		getCommandList().Get(), vertices.data(), vbByteSize, geo->VertexBufferUploader);

	geo->IndexBufferGPU = D3DUtil::createDefaultBuffer(getD3DDevice().Get(),
		getCommandList().Get(), indices.data(), ibByteSize, geo->IndexBufferUploader);

	geo->VertexByteStride = sizeof(Vertex);
	geo->VertexBufferByteSize = vbByteSize;
	geo->IndexFormat = DXGI_FORMAT_R16_UINT;
	geo->IndexBufferByteSize = ibByteSize;

	counter = 0;
	for (const auto& mesh : meshes)
	{
		geo->DrawArgs.push_back(submeshes[counter]);
		++counter;
	}

	mGeometries = std::move(geo);
}

void Visualizer3DCore::BuildPSOs()
{
	D3D12_GRAPHICS_PIPELINE_STATE_DESC opaquePsoDesc;

	//
	// PSO for opaque objects.
	//
	ZeroMemory(&opaquePsoDesc, sizeof(D3D12_GRAPHICS_PIPELINE_STATE_DESC));
	opaquePsoDesc.InputLayout = { mInputLayout.data(), (UINT)mInputLayout.size() };
	opaquePsoDesc.pRootSignature = mRootSignature.Get();
	opaquePsoDesc.VS =
	{
		reinterpret_cast<BYTE*>(mShaders["standardVS"]->GetBufferPointer()),
		mShaders["standardVS"]->GetBufferSize()
	};
	opaquePsoDesc.PS =
	{
		reinterpret_cast<BYTE*>(mShaders["opaquePS"]->GetBufferPointer()),
		mShaders["opaquePS"]->GetBufferSize()
	};
	opaquePsoDesc.RasterizerState = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
	opaquePsoDesc.BlendState = CD3DX12_BLEND_DESC(D3D12_DEFAULT);
	opaquePsoDesc.DepthStencilState = CD3DX12_DEPTH_STENCIL_DESC(D3D12_DEFAULT);
	opaquePsoDesc.SampleMask = UINT_MAX;
	opaquePsoDesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
	opaquePsoDesc.NumRenderTargets = 1;
	opaquePsoDesc.RTVFormats[0] = getBackBufferFormat();
	opaquePsoDesc.SampleDesc.Count = get4xMsaaState() ? 4 : 1;
	opaquePsoDesc.SampleDesc.Quality = get4xMsaaState() ? (get4xMsaaQuality() - 1) : 0;
	opaquePsoDesc.DSVFormat = getDepthStencilFormat();
	D3DUtil::throwIfFailed(getD3DDevice()->CreateGraphicsPipelineState(&opaquePsoDesc, IID_PPV_ARGS(&mOpaquePSO)));


	//PSO for transparent objects


	D3D12_GRAPHICS_PIPELINE_STATE_DESC transparentPsoDesc = opaquePsoDesc;

	D3D12_RENDER_TARGET_BLEND_DESC transparencyBlendDesc;
	transparencyBlendDesc.BlendEnable = true;
	transparencyBlendDesc.LogicOpEnable = false;
	transparencyBlendDesc.SrcBlend = D3D12_BLEND_SRC_ALPHA;
	transparencyBlendDesc.DestBlend = D3D12_BLEND_INV_SRC_ALPHA;
	transparencyBlendDesc.BlendOp = D3D12_BLEND_OP_ADD;
	transparencyBlendDesc.SrcBlendAlpha = D3D12_BLEND_ONE;
	transparencyBlendDesc.DestBlendAlpha = D3D12_BLEND_ZERO;
	transparencyBlendDesc.BlendOpAlpha = D3D12_BLEND_OP_ADD;
	transparencyBlendDesc.LogicOp = D3D12_LOGIC_OP_NOOP;
	transparencyBlendDesc.RenderTargetWriteMask = D3D12_COLOR_WRITE_ENABLE_ALL;

	transparentPsoDesc.BlendState.RenderTarget[0] = transparencyBlendDesc;
	D3DUtil::throwIfFailed(getD3DDevice()->CreateGraphicsPipelineState(&transparentPsoDesc, IID_PPV_ARGS(&mTransparentPSO)));


	// PSO for alpha tested objects


	//D3D12_GRAPHICS_PIPELINE_STATE_DESC alphaTestedPsoDesc = opaquePsoDesc;
	//alphaTestedPsoDesc.PS =
	//{
	//	reinterpret_cast<BYTE*>(mShaders["alphaTestedPS"]->GetBufferPointer()),
	//	mShaders["alphaTestedPS"]->GetBufferSize()
	//};
	//alphaTestedPsoDesc.RasterizerState.CullMode = D3D12_CULL_MODE_NONE;
	//D3DUtil::throwIfFailed(getD3DDevice()->CreateGraphicsPipelineState(&alphaTestedPsoDesc, IID_PPV_ARGS(&mPSOs["alphaTested"])));
}



void Visualizer3DCore::BuildFrameResources()
{
	mFrameResources.clear();
	if (rItemsCapacity == 0)
	{
		rItemsCapacity = static_cast<int>(mAllRitems.size());
	}
	if (materialsCapacity == 0)
	{
		materialsCapacity = static_cast<int>(mMaterials.size());
	}

	for (int i = 0; i < D3DUtil::getNFrameResources(); ++i)
	{
		mFrameResources.push_back(std::make_unique<FrameResource>(getD3DDevice().Get(),
			1, (UINT)rItemsCapacity, (UINT)materialsCapacity));
	}
}

void Visualizer3DCore::BuildMaterials()
{

}

void Visualizer3DCore::BuildRenderItems()
{
	mAllRitems.clear();
	mTransparentItems.clear();
	mOpaqueItems.clear();
	for (const auto& info : objects)
	{
		BuildOneRenderItem(info);
	}
}

void Visualizer3DCore::BuildOneRenderItem(Visualizer3DCore::ObjectInfo info)
{
	auto item = std::make_unique<RenderItem>();
	info.fillRenderItem(item.get());
	item->ObjCBIndex = static_cast<UINT>(mAllRitems.size());
	item->Mat = mMaterials[info.materialIndex].get();
	item->Geo = mGeometries.get();
	item->PrimitiveType = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST;
	item->IndexCount = item->Geo->DrawArgs[info.shapeIndex].IndexCount;
	item->StartIndexLocation = item->Geo->DrawArgs[info.shapeIndex].StartIndexLocation;
	item->BaseVertexLocation = item->Geo->DrawArgs[info.shapeIndex].BaseVertexLocation;

	if (item->isOpaque())
	{
		mOpaqueItems.push_back(item.get());
		item->underlyingLayerIndex = static_cast<int>(mOpaqueItems.size()) - 1;
	}
	else
	{
		mTransparentItems.push_back(item.get());
		item->underlyingLayerIndex = static_cast<int>(mTransparentItems.size()) - 1;
	}

	mAllRitems.push_back(std::move(item));
}

void Visualizer3DCore::DrawRenderItems(ID3D12GraphicsCommandList* cmdList, const std::vector<Visualizer3DCore::RenderItem*>& ritems)
{
	UINT objCBByteSize = D3DUtil::calcConstantBufferByteSize(sizeof(ObjectConstants));
	UINT matCBByteSize = D3DUtil::calcConstantBufferByteSize(sizeof(MaterialConstants));

	auto objectCB = mCurrFrameResource->ObjectCB->Resource();
	auto matCB = mCurrFrameResource->MaterialCB->Resource();

	//std::basic_string<wchar_t> message = L"Size: ";
	//message += ritems.size();
	//MessageBox(nullptr, message.data(), L"HR Failed", MB_OK);
	// For each render item...
	for (size_t i = 0; i < ritems.size(); ++i)
	{
		auto ri = ritems[i];

		auto vertexBufferView = ri->Geo->VertexBufferView();
		cmdList->IASetVertexBuffers(0, 1, &vertexBufferView);
		auto indexBufferView = ri->Geo->IndexBufferView();
		cmdList->IASetIndexBuffer(&indexBufferView);
		cmdList->IASetPrimitiveTopology(ri->PrimitiveType);

		D3D12_GPU_VIRTUAL_ADDRESS objCBAddress = objectCB->GetGPUVirtualAddress() + ri->ObjCBIndex * static_cast<unsigned long long>(objCBByteSize);
		D3D12_GPU_VIRTUAL_ADDRESS matCBAddress = matCB->GetGPUVirtualAddress() + ri->Mat->MatCBIndex * static_cast<unsigned long long>(matCBByteSize);

		cmdList->SetGraphicsRootConstantBufferView(0, objCBAddress);
		cmdList->SetGraphicsRootConstantBufferView(1, matCBAddress);

		cmdList->DrawIndexedInstanced(ri->IndexCount, 1, ri->StartIndexLocation, ri->BaseVertexLocation, 0);
	}
}

void Visualizer3DCore::InitializeDirectX()
{
	if (!D3DApp::initialize())
		throw std::runtime_error("Unable to initialize DirectX!");

	// Reset the command list to prep for initialization commands.
	D3DUtil::throwIfFailed(getCommandList()->Reset(getCommandAllocator().Get(), nullptr));

	// Get the increment size of a descriptor in this heap type.  This is hardware specific, 
	// so we have to query this information.
	mCbvSrvDescriptorSize = getD3DDevice()->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

	BuildRootSignature();
	BuildShadersAndInputLayout();
	BuildPSOs();

	// Execute the initialization commands.
	D3DUtil::throwIfFailed(getCommandList()->Close());

	ID3D12CommandList* cmdsLists[] = { getCommandList().Get() };
	getCommandQueue()->ExecuteCommandLists(_countof(cmdsLists), cmdsLists);

	// Wait until initialization is complete.
	flushCommandQueue();

	currentState = State::ReadyToBuildMeshes;
}

void Visualizer3DCore::BuildMeshes()
{
	flushCommandQueue();
	D3DUtil::throwIfFailed(getCommandList()->Reset(getCommandAllocator().Get(), nullptr));

	BuildShapeGeometry();

	//// Execute the initialization commands.
	D3DUtil::throwIfFailed(getCommandList()->Close());
	ID3D12CommandList* cmdsLists[] = { getCommandList().Get() };
	getCommandQueue()->ExecuteCommandLists(_countof(cmdsLists), cmdsLists);
	flushCommandQueue();

	meshesNeedUpdating = false;

	currentState = State::ReadyToBuildObjects;
}

void Visualizer3DCore::BuildObjects()
{
	// Go back if the meshes need work
	if (meshesNeedUpdating)
	{
		currentState = State::ReadyToBuildMeshes;
		return;
	}

	rescaleAll();

	if (autoCamera || oneTimeAutoCamera)
	{
		autoCameraAdjust();
		oneTimeAutoCamera = false;
	}

	flushCommandQueue();
	BuildRenderItems();
	BuildFrameResources();
	MarkAllDirty();
	objectsNeedRebuilding = false;

	if (isPaused())
	{
		currentState = State::Paused;
	}
	else
	{
		currentState = checkForNextState();
	}
}

void Visualizer3DCore::Unpause()
{
	currentState = checkForNextState();
}

void Visualizer3DCore::ProcessCommandQueue()
{
	while (!commandQueue.empty())
	{
		std::lock_guard<std::mutex> lock(queueLock);
		commandQueue.front()->execute(*this);
		commandQueue.pop_front();
	}
}

Visualizer3DCore::State Visualizer3DCore::checkForNextState() const
{
	if (meshesNeedUpdating)
	{
		return State::ReadyToBuildMeshes;
	}
	else if (objectsNeedRebuilding)
	{
		return State::ReadyToBuildObjects;
	}
	else
	{
		if (currentState == State::ReadyToUpdate)
		{
			return State::ReadyToDraw;
		}
		else
		{
			return State::ReadyToUpdate;
		}
	}
}

void Visualizer3DCore::autoCameraAdjust()
{
	//float cameraDistance = 1e-7;

	//camera.lookAt(XMFLOAT3(cameraDistance, cameraDistance, cameraDistance), XMFLOAT3(0, 0, 0), XMFLOAT3(-1, -1, 1));

	//auto viewMatrix = camera.getView();

	float cameraTanX = std::tan(camera.getFovX() / 2);
	float cameraTanY = std::tan(camera.getFovY() / 2);

	//XMFLOAT3 zAxis(0, 0, 1);
	//auto rotationMatrixZ = XMMatrixRotationAxis(XMLoadFloat3(&zAxis), -MathHelper::Pi / 4);
	//XMFLOAT3 yAxis(0, 1, 0);
	//auto rotationMatrixY = XMMatrixRotationAxis(XMLoadFloat3(&yAxis), -.9553);
	//auto rotationMatrix = XMMatrixMultiply(rotationMatrixZ, rotationMatrixY);

	// Many thanks to https://math.stackexchange.com/questions/180418/calculate-rotation-matrix-to-align-vector-a-to-vector-b-in-3d
	//const float root3recip = 1.0f / std::sqrt(3.0f);
	//const float q = std::sqrt(3.0f) / (std::sqrt(3.0f) - 1.0f);
	//const float qover3 = q / 3.0f;
	//XMMATRIX rotationMatrix(1.0f - qover3, -qover3, -root3recip, 0.0f,
	//	-qover3, 1.0f - qover3, -root3recip, 0.0f,
	//	root3recip, root3recip, 1.0f - 2 * qover3, 0.0f,
	//	0.0f, 0.0f, 0.0f, 1.0f);

	// Pre-programmed rotation matrix positioned along (1, 1, 1) pointing at the center
	XMMATRIX rotationMatrix(.707106888f, -.408248305f, -.577350318f, 0, -.707106888f, -.408248305f, -.577350318f, 0, 0, .816496611f, -.577350318f, 0, 0, 0, 0, 1.0f);
	auto translationMatrix = XMMatrixTranslationFromVector(-center);
	rotationMatrix = translationMatrix * rotationMatrix;

	const float bufferFactor = 1.1f;

	// Who is defining these to begin with?
#undef max
#undef min
	float minZ = std::numeric_limits<float>::max();
	float maxZ = std::numeric_limits<float>::lowest();
	// Z0 is the camera position along the rotated z axis
	float maxZ0 = std::numeric_limits<float>::lowest();

	for (const auto& obj : objects)
	{
		XMMATRIX transformMatrix;
		obj.getMatrix(transformMatrix);
		for (const auto& vertex : storedVertices[obj.shapeIndex])
		{
			XMVECTOR localPosition = XMLoadFloat3(&vertex.Pos);
			auto globalPosition = XMVector3Transform(localPosition, transformMatrix);
			//auto rotatedPosition = XMVector3Transform(globalPosition, viewMatrix);
			auto rotatedPosition = XMVector3Transform(globalPosition, rotationMatrix);

			float z = XMVectorGetZ(rotatedPosition);
			if (z < minZ)
			{
				minZ = z;
			}
			if (z > maxZ)
			{
				maxZ = z;
			}
			float z0x = std::abs(XMVectorGetX(rotatedPosition)) / cameraTanX - z;
			float z0y = std::abs(XMVectorGetY(rotatedPosition)) / cameraTanY - z;

			if (z0x > maxZ0)
			{
				maxZ0 = z0x;
			}
			if (z0y > maxZ0)
			{
				maxZ0 = z0y;
			}

			//if (auto zView = XMVectorGetZ(rotatedPosition); zView < camera.GetNearZ())
			//{
			//	cameraDistance += (camera.GetNearZ() - zView) / std::sqrt(3.0f); // Will be positive
			//	camera.lookAt(XMFLOAT3(cameraDistance, cameraDistance, cameraDistance), XMFLOAT3(0, 0, 0), XMFLOAT3(-1, -1, 1));
			//	viewMatrix = camera.getView();
			//	rotatedPosition = XMVector3Transform(globalPosition, viewMatrix);

			//	//zView = XMVectorGetZ(rotatedPosition);
			//	//assert(zView > camera.GetNearZ());
			//}

			//float ratioX = std::abs(XMVectorGetX(rotatedPosition) / XMVectorGetZ(rotatedPosition));
			//if (ratioX > cameraTanX)
			//{
			//	float newDistance = std::abs(XMVectorGetX(rotatedPosition) / cameraTanX);
			//	cameraDistance += newDistance - XMVectorGetZ(rotatedPosition);
			//	camera.lookAt(XMFLOAT3(cameraDistance, cameraDistance, cameraDistance), XMFLOAT3(0, 0, 0), XMFLOAT3(-1, -1, 1));
			//	viewMatrix = camera.getView();
			//	rotatedPosition = XMVector3Transform(globalPosition, viewMatrix);
			//}
			//float ratioY = std::abs(XMVectorGetY(rotatedPosition) / XMVectorGetZ(rotatedPosition));
			//if (ratioY > cameraTanY)
			//{
			//	float newDistance = std::abs(XMVectorGetY(rotatedPosition) / cameraTanY);
			//	cameraDistance += newDistance - XMVectorGetZ(rotatedPosition);
			//	camera.lookAt(XMFLOAT3(cameraDistance, cameraDistance, cameraDistance), XMFLOAT3(0, 0, 0), XMFLOAT3(-1, -1, 1));
			//	viewMatrix = camera.getView();
			//	rotatedPosition = XMVector3Transform(globalPosition, viewMatrix);
			//}

			//if (auto zView = XMVectorGetZ(rotatedPosition); zView > camera.GetFarZ())
			//{
			//	camera.setLens(camera.getFovY(), camera.GetAspect(), camera.GetNearZ(), zView * 2);
			//}

		}
	}

	XMFLOAT3 upDirection(0, 0, 1);

	float frustrumDepth = maxZ - minZ;
	float bufferedFrustrumDepth = frustrumDepth * bufferFactor;
	float zBuffer = (bufferedFrustrumDepth - frustrumDepth) / 2;

	// Camera movement settings
	cameraAccel = 100 * frustrumDepth;
	maxSpeed = 100 * frustrumDepth;
	cameraDrag = 500 * frustrumDepth;

	const float nearPlaneScale = .5;
	const float farPlaneScale = 10;

	float nearPlane = (maxZ0 * bufferFactor + minZ - zBuffer) * nearPlaneScale;
	float farPlane = (maxZ0 * bufferFactor + maxZ + zBuffer) * farPlaneScale;

	if (farPlane - nearPlane < .001)
	{
		farPlane += 1;
	}

	float eachComponent = static_cast<float>(maxZ0 / std::sqrt(3) * bufferFactor);

	XMFLOAT3 cameraPosition(eachComponent, eachComponent, eachComponent);
	auto cameraPosVec = XMLoadFloat3(&cameraPosition);
	cameraPosVec += center;
	XMStoreFloat3(&cameraPosition, cameraPosVec);

	XMFLOAT3 target;
	XMStoreFloat3(&target, center);

	camera.lookAt(cameraPosition, target, upDirection);
	camera.setLens(camera.getFovY(), camera.getAspect(), nearPlane, farPlane);

	// Check it:
	auto viewMatrix = camera.getView();
	auto projectionMatrix = camera.getProj();
	for (const auto& obj : objects)
	{
		XMMATRIX transformMatrix;
		obj.getMatrix(transformMatrix);
		for (const auto& vertex : storedVertices[obj.shapeIndex])
		{
			XMVECTOR localPosition = XMLoadFloat3(&vertex.Pos);
			//XMFLOAT3 test(0, 0, 1);
			//localPosition = XMLoadFloat3(&test);
			//transformMatrix = XMMatrixIdentity();
			auto globalPosition = XMVector3Transform(localPosition, transformMatrix);
			auto rotatedPosition = XMVector3Transform(globalPosition, viewMatrix);
			auto projectedPosition = XMVector3Transform(rotatedPosition, projectionMatrix);

			assert(XMVectorGetZ(rotatedPosition) > camera.getNearZ());
			assert(XMVectorGetZ(rotatedPosition) < camera.getFarZ());
			assert(XMVectorGetX(rotatedPosition) / XMVectorGetZ(rotatedPosition) < std::tan(camera.getFovX() / 2));
			assert(XMVectorGetY(rotatedPosition) / XMVectorGetZ(rotatedPosition) < std::tan(camera.getFovY() / 2));

			//assert(XMVectorGetZ(projectedPosition) >= 0 && XMVectorGetZ(projectedPosition) <= XMVectorGetZ(projectedPosition));
			//assert(XMVectorGetX(projectedPosition) >= -XMVectorGetZ(projectedPosition) && XMVectorGetX(projectedPosition) <= XMVectorGetZ(projectedPosition));
			//assert(XMVectorGetY(projectedPosition) >= -XMVectorGetZ(projectedPosition) && XMVectorGetY(projectedPosition) <= XMVectorGetZ(projectedPosition));
		}
	}
}

float Visualizer3DCore::getLargestDistanceFromCenter() const
{
	float largestDistanceSquared = 0;
	for (const auto& obj : objects)
	{
		XMMATRIX transformMatrix;
		obj.getMatrix(transformMatrix);
		for (const auto& vertex : storedVertices[obj.shapeIndex])
		{
			XMVECTOR position = XMLoadFloat3(&vertex.Pos);
			auto newVector = XMVector3Transform(position, transformMatrix);
			auto lengthSquaredVector = XMVector3LengthSq(newVector);
			float lengthSquared = XMVectorGetX(lengthSquaredVector);
			if (lengthSquared > largestDistanceSquared)
				largestDistanceSquared = lengthSquared;
		}
	}
	return std::sqrt(largestDistanceSquared);
}

void Visualizer3DCore::calculateCenter()
{
	float minX = std::numeric_limits<float>::max();
	float maxX = std::numeric_limits<float>::lowest();
	float minY = std::numeric_limits<float>::max();
	float maxY = std::numeric_limits<float>::lowest();
	float minZ = std::numeric_limits<float>::max();
	float maxZ = std::numeric_limits<float>::lowest();

	for (const auto& obj : objects)
	{
		XMMATRIX transformMatrix;
		obj.getMatrix(transformMatrix);
		for (const auto& vertex : storedVertices[obj.shapeIndex])
		{
			XMVECTOR position = XMLoadFloat3(&vertex.Pos);
			auto newVector = XMVector3Transform(position, transformMatrix);
			XMFLOAT3 finalPosition;
			XMStoreFloat3(&finalPosition, newVector);

			if (finalPosition.x < minX)
			{
				minX = finalPosition.x;
			}
			if (finalPosition.x > maxX)
			{
				maxX = finalPosition.x;
			}
			if (finalPosition.y < minY)
			{
				minY = finalPosition.y;
			}
			if (finalPosition.y > maxY)
			{
				maxY = finalPosition.y;
			}
			if (finalPosition.z < minZ)
			{
				minZ = finalPosition.z;
			}
			if (finalPosition.z > maxZ)
			{
				maxZ = finalPosition.z;
			}
		}
	}
	XMFLOAT3 temp((maxX + minX) / 2, (maxY + minY) / 2, (maxZ + minZ) / 2);
	center = XMLoadFloat3(&temp);
}

void Visualizer3DCore::rescaleAll()
{
	// Start by resetting scale factor
	ObjectInfo::globalScaleFactor = 1;

	float maxDistance = getLargestDistanceFromCenter();
	const float maxAllowed = 1e5f;
	const float minAllowed = 1e-5f;
	if (maxDistance > maxAllowed || maxDistance < minAllowed)
	{
		float newScaleFactor = 1 / maxDistance;
		//camera.scaleAll(newScaleFactor / ObjectInfo::globalScaleFactor);

		ObjectInfo::globalScaleFactor = newScaleFactor;
		center = XMVectorScale(center, newScaleFactor);
	}

	calculateCenter();
}

void Visualizer3DCore::rescaleZ()
{
	auto viewMatrix = camera.getView();

	//float minZ = camera.GetNearZ();
	//float maxZ = camera.GetFarZ();
	float minZ = std::numeric_limits<float>::max();
	float maxZ = std::numeric_limits<float>::lowest();

	for (const auto& obj : objects)
	{
		XMMATRIX transformMatrix;
		obj.getMatrix(transformMatrix);
		for (const auto& vertex : storedVertices[obj.shapeIndex])
		{
			XMVECTOR position = XMLoadFloat3(&vertex.Pos);
			auto newVector = XMVector3Transform(position, transformMatrix);
			auto rotatedVector = XMVector3Transform(newVector, viewMatrix);
			float z = XMVectorGetZ(rotatedVector);

			if (z > 0 && z < minZ)
			{
				minZ = z;
			}
			if (z > maxZ)
			{
				maxZ = z;
			}
		}
	}
	if (minZ == std::numeric_limits<float>::max())
		return;

	if (minZ != maxZ)
	{
		camera.setLens(camera.getFovY(), camera.getAspect(), minZ, maxZ);
	}
}

void Visualizer3DCore::MarkAllDirty()
{
	for (auto& item : mAllRitems)
	{
		item->NumFramesDirty = D3DUtil::getNFrameResources();
	}
	for (auto& material : mMaterials)
	{
		material->NumFramesDirty = D3DUtil::getNFrameResources();
	}
}

LRESULT Visualizer3DCore::ProcProxy(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	if (message == WM_CREATE)
	{
		LPCREATESTRUCT pcs = (LPCREATESTRUCT)lParam;
		auto pDemoApp = (Visualizer3DCore*)pcs->lpCreateParams;

		::SetWindowLongPtrW(
			hWnd,
			GWLP_USERDATA,
			reinterpret_cast<LONG_PTR>(pDemoApp)
		);

		return 1;
	}
	else
	{
		auto pDemoApp = reinterpret_cast<Visualizer3DCore*>(static_cast<LONG_PTR>(
			::GetWindowLongPtrW(
				hWnd,
				GWLP_USERDATA
			)));
		if (pDemoApp)
		{
			return pDemoApp->processMessage(hWnd, message, wParam, lParam);
		}
		else
		{
			return DefWindowProc(hWnd, message, wParam, lParam);;
		}

	}
}

void Visualizer3DCore::addMeshData(int index, const GeometryGenerator::MeshData& mesh)
{
	if (meshes.size() <= index)
	{
		meshes.resize(static_cast<std::vector<GeometryGenerator::MeshData>::size_type>(index) + 1);
	}
	meshes[index] = mesh;

	meshesNeedUpdating = true;
	objectsNeedRebuilding = true;
}

void Visualizer3DCore::addMaterial(int index, XMVECTORF32 color,
	float fresnel, float roughness)
{
	auto material = std::make_unique<Material>();
	material->MatCBIndex = static_cast<int>(mMaterials.size());
	material->DiffuseSrvHeapIndex = static_cast<int>(mMaterials.size());
	material->DiffuseAlbedo = XMFLOAT4(color);
	material->FresnelR0 = XMFLOAT3(fresnel, fresnel, fresnel);
	material->Roughness = roughness;

	if (mMaterials.size() <= index)
	{
		mMaterials.resize(static_cast<std::vector<std::unique_ptr<Material>>::size_type>(index) + 1);
	}

	mMaterials[index] = std::move(material);

	if (mMaterials.size() > materialsCapacity)
	{
		materialsCapacity *= 2;
		objectsNeedRebuilding = true;
	}
}

void Visualizer3DCore::addMaterial(int index, float colorR,
	float colorG, float colorB, float alpha, float fresnel, float roughness)
{
	XMVECTORF32 color{ colorR, colorG, colorB, alpha };
	addMaterial(index, color, fresnel, roughness);
}

void Visualizer3DCore::changeMaterialColor(int index, XMVECTORF32 color)
{
	mMaterials[index]->DiffuseAlbedo = XMFLOAT4(color);
	objectsNeedRebuilding = true;
}

void Visualizer3DCore::addObject(int index, DirectX::XMFLOAT3 scale, DirectX::XMFLOAT3X3 rotation,
	DirectX::XMFLOAT3 position, int materialIndex, int shapeIndex)
{
	ObjectInfo info(scale, rotation, position, materialIndex, shapeIndex);

	objects.push_back(info);
	objectIndexMap[index] = static_cast<int>(objects.size()) - 1;

	if (objects.size() > rItemsCapacity)
	{
		rItemsCapacity *= 2;
		objectsNeedRebuilding = true;
	}

	if (!objectsNeedRebuilding)
		BuildOneRenderItem(info);

}

DirectX::XMFLOAT3 Visualizer3DCore::getObjectPosition(int index) const
{
	return objects.at(objectIndexMap.at(index)).translation;
}

void Visualizer3DCore::moveObject(int index, DirectX::XMFLOAT3 newPosition)
{
	int internalIndex = objectIndexMap[index];
	auto& info = objects.at(internalIndex);
	info.updateTranslation(newPosition);
	if (!objectsNeedRebuilding)
	{
		auto renderItem = mAllRitems.at(internalIndex).get();
		info.fillRenderItem(renderItem);
		renderItem->NumFramesDirty = D3DUtil::getNFrameResources();
	}
}

void Visualizer3DCore::transformObject(int index, DirectX::XMFLOAT3 newScale, DirectX::XMFLOAT3X3 newRotation, DirectX::XMFLOAT3 newPosition)
{
	int internalIndex = objectIndexMap[index];
	auto& info = objects.at(internalIndex);
	info.updateTransformation(newScale, newRotation, newPosition);
	if (!objectsNeedRebuilding)
	{
		auto renderItem = mAllRitems.at(internalIndex).get();
		info.fillRenderItem(renderItem);
		renderItem->NumFramesDirty = D3DUtil::getNFrameResources();
	}
}

void Visualizer3DCore::removeObject(int index)
{
	int internalIndex = objectIndexMap[index];
	if (!objectsNeedRebuilding)
	{
		auto specialIndex = mAllRitems[internalIndex]->underlyingLayerIndex;
		if (mAllRitems[internalIndex]->isOpaque())
		{
			mOpaqueItems.erase(mOpaqueItems.begin() + specialIndex);
			for (auto& item : mAllRitems)
			{
				if (item->isOpaque() && item->underlyingLayerIndex >= specialIndex)
				{
					--item->underlyingLayerIndex;
				}
			}
		}
		else
		{
			mTransparentItems.erase(mTransparentItems.begin() + specialIndex);
			for (auto& item : mAllRitems)
			{
				if (!item->isOpaque() && item->underlyingLayerIndex >= specialIndex)
				{
					--item->underlyingLayerIndex;
				}
			}
		}

		mAllRitems.erase(mAllRitems.begin() + internalIndex);
	}
	// Note that this changes all the indices of other objects as well!  Beware!
	objects.erase(objects.begin() + internalIndex);
	// Have to adjust the map of indices to match
	for (auto& pair : objectIndexMap)
	{
		if (pair.second > internalIndex)
		{
			--pair.second;
		}
	}
	//static int nRemovals = 0;
	//if (++nRemovals > 100)
	//{
	objectsNeedRebuilding = true;
	//	nRemovals = 0;
	//}
}

void Visualizer3DCore::removeAllObjects()
{
	objects.clear();

	mAllRitems.clear();
	mOpaqueItems.clear();
	mTransparentItems.clear();
}

void Visualizer3DCore::clearAll()
{
	removeAllObjects();
	rItemsCapacity = 0;
	mMaterials.clear();
	meshes.clear();
	meshesNeedUpdating = true;
	objectsNeedRebuilding = true;
	rItemsCapacity = 0;
	currentState = State::ReadyToBuildMeshes;
}

void Visualizer3DCore::moveCamera(DirectX::XMFLOAT3 newPosition)
{
	camera.setPosition(newPosition);
}

void Visualizer3DCore::moveCamera(DirectX::XMFLOAT3 newPosition, DirectX::XMFLOAT3 lookDirection, DirectX::XMFLOAT3 upDirection)
{
	auto newTarget = XMVectorAdd(XMLoadFloat3(&lookDirection), XMLoadFloat3(&newPosition));
	camera.lookAt(XMLoadFloat3(&newPosition), newTarget, XMLoadFloat3(&upDirection));
}

void Visualizer3DCore::lookAt(DirectX::XMFLOAT3 newPosition, DirectX::XMFLOAT3 target, DirectX::XMFLOAT3 upDirection)
{
	camera.lookAt(newPosition, target, upDirection);
}

void Visualizer3DCore::adjustCameraLens(float fieldOfViewY, float aspectRatio, float nearZ, float farZ)
{
	camera.setLens(fieldOfViewY, aspectRatio, nearZ, farZ);
}

void Visualizer3DCore::singleAutoCameraAdjustment()
{
	oneTimeAutoCamera = true;
}

void Visualizer3DCore::doCommand(const std::shared_ptr<VisualizerCommand> command)
{
	// Wait for execution to finish
	//while (currentlyExecuting);
	std::lock_guard<std::mutex> lock(queueLock);
	commandQueue.push_back(command);
}

bool Visualizer3DCore::WindowRegister(LPCWSTR ClassName)
{
	WNDCLASSEX wcex;
	wcex.cbSize = sizeof(WNDCLASSEX);
	wcex.style = CS_HREDRAW | CS_VREDRAW;
	wcex.lpfnWndProc = Visualizer3DCore::ProcProxy;		//Different
	wcex.cbClsExtra = 0;
	wcex.cbWndExtra = sizeof(LONG_PTR);		//Different
	wcex.hInstance = HINST_THISCOMPONENT;
	wcex.hIcon = NULL;
	wcex.hCursor = NULL;
	wcex.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
	wcex.lpszMenuName = NULL;
	wcex.lpszClassName = ClassName;
	wcex.hIconSm = NULL;

	setApplicationInstance(HINST_THISCOMPONENT);

	return RegisterClassEx(&wcex);
}

HWND Visualizer3DCore::WindowMake(LPCWSTR ClassName, DWORD style, int height, int width, HWND parent)
{
	hwnd = CreateWindowEx(
		0,
		ClassName,
		L"Visualizer",
		style,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		width,
		height,
		parent,
		NULL,
		HINST_THISCOMPONENT,
		this
	);
	auto a = GetLastError();
	setMainWindowHandle(hwnd);
	return hwnd;
}

XMFLOAT3 Visualizer3DCore::ConvertToXMFLOAT3(float array[])
{
	return XMFLOAT3(array[0], array[1], array[2]);
}

Visualizer3DCore app(HINSTANCE(0));

extern "C"
{
	__declspec(dllexport) bool RegisterWindow(LPCWSTR ClassName)
	{
		return app.WindowRegister(ClassName);
	}

	__declspec(dllexport) HWND MakeWindow(LPCWSTR ClassName, DWORD style, int height, int width, HWND parent)
	{
		return app.WindowMake(ClassName, style, height, width, parent);
	}

	__declspec(dllexport) bool CheckRegistration(LPCWSTR ClassName)
	{
		WNDCLASSEX* temp = new WNDCLASSEX;
		return GetClassInfoEx(HINST_THISCOMPONENT, ClassName, temp);
	}

	__declspec(dllexport) void SetupDirectX()
	{
		try
		{
			//app.initialize();

			app.run();
		}
		catch (DxException& ex)
		{
			MessageBox(app.mainWindow(), ex.ToString().data(), L"DirectX error!", MB_OK);
		}
		catch (std::exception& ex)
		{
			MessageBox(app.mainWindow(), D3DUtil::ansiToWString(ex.what()).data(), L"Internal error!", MB_OK);
		}
		catch (...)
		{
			MessageBox(app.mainWindow(), L"Some weird other exception?", L"Unknown error!", MB_OK);
		}
	}

	// Vertices are packed in sets of three floats, so vertices[] and normals[]
	// are both of size 3*nVertices
	__declspec(dllexport) void AddShape(int index, int nVertices, float vertices[],
		float normals[], int nTriangleIndices, std::uint32_t triangles[])
	{
		GeometryGenerator::MeshData newMesh;
		for (int iVertex = 0; iVertex < nVertices; ++iVertex)
		{
			int i = iVertex * 3;
			GeometryGenerator::Vertex vertex(vertices[i], vertices[i + 1],
				vertices[i + 2], normals[i], normals[i + 1],
				normals[i + 2], 0.0f, -0.0f, 0.0f, 0.0f, 0.0f);
			newMesh.Vertices.push_back(vertex);
		}

		for (int iTriangle = 0; iTriangle < nTriangleIndices; ++iTriangle)
		{
			newMesh.Indices32.push_back(triangles[iTriangle]);
		}

		app.doCommand(std::make_shared<AddMeshDataCommand>(index, newMesh));
	}

	__declspec(dllexport) void AddMaterial(int index, float colorR, float colorG,
		float colorB, float alpha, float fresnel, float roughness)
	{
		XMVECTORF32 color{ colorR, colorG, colorB, alpha };
		app.doCommand(std::make_shared<AddMaterialCommand>(index, color, fresnel, roughness));
	}

	__declspec(dllexport) void ChangeMaterialColor(int index, float colorR, float colorG,
		float colorB, float alpha)
	{
		app.doCommand(std::make_shared<ChangeMaterialColorCommand>(index, colorR, colorG, colorB, alpha));
	}

	// position[] and scale[] are both three-element arrays (vectors)
	// rotation[] is a nine-element 3x3 array
	__declspec(dllexport) void AddObject(int index, float scale[], float rotation[], float position[],
		int shape, int material)
	{
		app.doCommand(std::make_shared<AddObjectCommand>(index, Visualizer3DCore::ConvertToXMFLOAT3(scale),
			DirectX::XMFLOAT3X3(rotation), Visualizer3DCore::ConvertToXMFLOAT3(position), material, shape));
	}

	// position[] is a three-element array (vector)
	__declspec(dllexport) void GetObjectPosition(int index, float position[])
	{
		auto pos = app.getObjectPosition(index);
		position[0] = pos.x;
		position[1] = pos.y;
		position[2] = pos.z;
	}

	__declspec(dllexport) void MoveObject(int index, float newPosition[])
	{
		app.doCommand(std::make_shared<MoveObjectCommand>(index, Visualizer3DCore::ConvertToXMFLOAT3(newPosition)));
	}

	__declspec(dllexport) void TransformObject(int index, float scale[], float rotation[],
		float position[])
	{
		app.doCommand(std::make_shared<TransformObjectCommand>(index, Visualizer3DCore::ConvertToXMFLOAT3(scale), DirectX::XMFLOAT3X3(rotation),
			Visualizer3DCore::ConvertToXMFLOAT3(position)));
	}

	__declspec(dllexport) void RemoveObject(int index)
	{
		app.doCommand(std::make_shared<RemoveObjectCommand>(index));
	}

	__declspec(dllexport) void Clear()
	{
		app.doCommand(std::make_shared<ClearAllCommand>());
	}

	__declspec(dllexport) void SetAutoCamera(bool value)
	{
		app.setAutoCamera(value);
	}

	__declspec(dllexport) void AutoCameraAdjust()
	{
		app.singleAutoCameraAdjustment();
	}

	__declspec(dllexport) void MoveCamera(float newPosition[])
	{
		app.moveCamera(Visualizer3DCore::ConvertToXMFLOAT3(newPosition));
	}

	__declspec(dllexport) void MoveAndTurnCamera(float newPosition[], float lookDirection[], float upDirection[])
	{
		app.moveCamera(Visualizer3DCore::ConvertToXMFLOAT3(newPosition), Visualizer3DCore::ConvertToXMFLOAT3(lookDirection),
			Visualizer3DCore::ConvertToXMFLOAT3(upDirection));
	}

	__declspec(dllexport) void lookAt(float newPosition[], float target[], float upDirection[])
	{
		app.lookAt(Visualizer3DCore::ConvertToXMFLOAT3(newPosition), Visualizer3DCore::ConvertToXMFLOAT3(target),
			Visualizer3DCore::ConvertToXMFLOAT3(upDirection));
	}

	__declspec(dllexport) void AdjustLens(float fieldOfViewY, float aspectRatio, float nearZ, float farZ)
	{
		app.adjustCameraLens(fieldOfViewY, aspectRatio, nearZ, farZ);
	}

	__declspec(dllexport) void SetPauseDrawingState(bool value)
	{
		app.setPaused(value);
	}

	// These three-dimensional arrays need to be initialized to the right size already
	__declspec(dllexport) void GetCameraPosition(float position[], float lookDirection[], float upDirection[])
	{
		auto& camera = app.getCamera();
		auto camPosition = camera.getPosition3f();
		position[0] = camPosition.x;
		position[1] = camPosition.y;
		position[2] = camPosition.z;

		auto camLook = camera.getLook3f();
		lookDirection[0] = camLook.x;
		lookDirection[1] = camLook.y;
		lookDirection[2] = camLook.z;

		auto camUp = camera.getUp3f();
		upDirection[0] = camUp.x;
		upDirection[1] = camUp.y;
		upDirection[2] = camUp.z;
	}
}

float Visualizer3DCore::ObjectInfo::globalScaleFactor = 1;

Visualizer3DCore::ObjectInfo::ObjectInfo(DirectX::XMFLOAT3 scaleVec, DirectX::XMFLOAT3X3 rotationVec,
	DirectX::XMFLOAT3 translationVec, int material, int shape)
{
	updateTransformation(scaleVec, rotationVec, translationVec);
	materialIndex = material;
	shapeIndex = shape;
}

void Visualizer3DCore::ObjectInfo::fillRenderItem(RenderItem* renderItem) const
{
	XMMATRIX finalMatrix;
	getMatrix(finalMatrix);
	XMStoreFloat4x4(&(renderItem->World), finalMatrix);
}

void Visualizer3DCore::ObjectInfo::getMatrix(XMMATRIX& result) const
{
	auto scaleMatrix = XMMatrixScaling(scale.x * globalScaleFactor, scale.y * globalScaleFactor, scale.z * globalScaleFactor);
	auto translationMatrix = XMMatrixTranslation(translation.x * globalScaleFactor, translation.y * globalScaleFactor, translation.z * globalScaleFactor);
	XMFLOAT4X4 matrix;
	XMStoreFloat4x4(&matrix, scaleMatrix * rotation * translationMatrix);
	result = XMLoadFloat4x4(&matrix);
}

void Visualizer3DCore::ObjectInfo::updateTransformation(DirectX::XMFLOAT3 scaleVec, DirectX::XMFLOAT3X3 rotationVec, DirectX::XMFLOAT3 translationVec)
{
	scale = scaleVec;
	translation = translationVec;
	rotation = DirectX::XMLoadFloat3x3(&rotationVec);
}
