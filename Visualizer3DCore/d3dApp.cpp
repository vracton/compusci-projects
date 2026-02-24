//***************************************************************************************
// d3dApp.cpp by Frank Luna (C) 2015 All Rights Reserved.
//***************************************************************************************

#include "D3DApp.h"
#include <WindowsX.h>

using Microsoft::WRL::ComPtr;
using namespace std;
using namespace DirectX;

LRESULT CALLBACK
D3DApp::mainWindowsProcedure(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	// Forward hwnd on because we can get messages (e.g., WM_CREATE)
	// before CreateWindow returns, and thus before mhMainWnd is valid.
    return D3DApp::getApp()->processMessage(hwnd, msg, wParam, lParam);
}

D3DApp* D3DApp::mApp = nullptr;

D3DApp* D3DApp::getApp()
{
    return mApp;
}

D3DApp::D3DApp(HINSTANCE hInstance) :
	mhAppInst(hInstance)
{
    // Only one D3DApp can be constructed.
    assert(mApp == nullptr);
    mApp = this;
}

D3DApp::~D3DApp()
{
	if(md3dDevice != nullptr)
		flushCommandQueue();
}

HINSTANCE D3DApp::appInstance()const
{
	return mhAppInst;
}

HWND D3DApp::mainWindow()const
{
	return mhMainWnd;
}

float D3DApp::aspectRatio()const
{
	return static_cast<float>(mClientWidth) / mClientHeight;
}

bool D3DApp::get4xMsaaState()const
{
    return m4xMsaaState;
}

void D3DApp::set4xMsaaState(bool value)
{
    if(m4xMsaaState != value)
    {
        m4xMsaaState = value;

        // Recreate the swapchain and buffers with new multisample settings.
        createSwapChain();
        onResize();
    }
}

int D3DApp::run()
{
	MSG msg = {0};
 
	mTimer.Reset();

	while(msg.message != WM_QUIT)
	{
		// If there are Window messages then process them.
		if(PeekMessage(&msg, 0, 0, 0, PM_REMOVE))
		{
            TranslateMessage(&msg);
            DispatchMessage(&msg);
		}
		// Otherwise, do animation/game stuff.
		else
        {	
			mTimer.Tick();		
			runStateMachine(mTimer);
        }
    }

	return static_cast<int>(msg.wParam);
}

bool D3DApp::initialize()
{
	if(!initDirect3D())
		return false;

    // Do the initial resize code.
    onResize();

	return true;
}
 
void D3DApp::createRtvAndDsvDescriptorHeaps()
{
	// Create RTV heap
	D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc
	{
		D3D12_DESCRIPTOR_HEAP_TYPE_RTV,  // Type
		swapChainBufferCount,            // NumDescriptors
		D3D12_DESCRIPTOR_HEAP_FLAG_NONE, // Flags
		0                                // NodeMask
	};

    D3DUtil::throwIfFailed(md3dDevice->CreateDescriptorHeap(
        &rtvHeapDesc, IID_PPV_ARGS(mRtvHeap.GetAddressOf())));

	// Create DSV heap
	D3D12_DESCRIPTOR_HEAP_DESC dsvHeapDesc
	{
		D3D12_DESCRIPTOR_HEAP_TYPE_DSV,  // Type
		1,                               // NumDescriptors
		D3D12_DESCRIPTOR_HEAP_FLAG_NONE, // Flags
		0                                // NodeMask
	};

    D3DUtil::throwIfFailed(md3dDevice->CreateDescriptorHeap(
        &dsvHeapDesc, IID_PPV_ARGS(mDsvHeap.GetAddressOf())));
}

void D3DApp::onResize()
{
	assert(md3dDevice);
	assert(mSwapChain);
    assert(mDirectCmdListAlloc);

	// Flush before changing any resources.
	flushCommandQueue();

    D3DUtil::throwIfFailed(mCommandList->Reset(mDirectCmdListAlloc.Get(), nullptr));

	// Release the previous resources we will be recreating.
	for (int i = 0; i < swapChainBufferCount; ++i)
		mSwapChainBuffer[i].Reset();
    mDepthStencilBuffer.Reset();
	
	// Resize the swap chain.
    D3DUtil::throwIfFailed(mSwapChain->ResizeBuffers(
		swapChainBufferCount, 
		mClientWidth, mClientHeight, 
		mBackBufferFormat, 
		DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH));

	mCurrBackBuffer = 0;
 
	// Create render target views (RTVs) for the swap chain buffers.
	CD3DX12_CPU_DESCRIPTOR_HANDLE rtvHeapHandle(mRtvHeap->GetCPUDescriptorHandleForHeapStart());
	for (UINT i = 0; i < swapChainBufferCount; i++)
	{
		D3DUtil::throwIfFailed(mSwapChain->GetBuffer(i, IID_PPV_ARGS(&mSwapChainBuffer[i])));
		md3dDevice->CreateRenderTargetView(mSwapChainBuffer[i].Get(), nullptr, rtvHeapHandle);
		rtvHeapHandle.Offset(1, mRtvDescriptorSize);
	}

    // Create the depth/stencil buffer and view.
	D3D12_RESOURCE_DESC depthStencilDesc
	{
		D3D12_RESOURCE_DIMENSION_TEXTURE2D, // Dimension
		0,                                  // Alignment
		static_cast<UINT64>(mClientWidth),  // Width
		static_cast<UINT>(mClientHeight),   // Height
		1,                                  // DepthOrArraySize
		1,                                  // MipLevels
		mDepthStencilFormat,                // Format
		{
			m4xMsaaState ? 4u : 1u, 		  // Count
			static_cast<UINT>(m4xMsaaState) ? (static_cast<UINT>(m4xMsaaQuality) - 1) : 0	// Quality
		}, // SampleDesc
		D3D12_TEXTURE_LAYOUT_UNKNOWN,       // Layout
		D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL // Flags
	};

	// We cannot clear a resource that has a typeless format, so we must create a depth/stencil
	D3D12_CLEAR_VALUE optClear
	{
		mDepthStencilFormat, // Format
		{ 1.0f, 0 }          // DepthStencil
	};

	// Create the depth/stencil buffer.
	auto heapProperties = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_DEFAULT);
    D3DUtil::throwIfFailed(md3dDevice->CreateCommittedResource(
        &heapProperties,
		D3D12_HEAP_FLAG_NONE,
        &depthStencilDesc,
		D3D12_RESOURCE_STATE_COMMON,
        &optClear,
        IID_PPV_ARGS(mDepthStencilBuffer.GetAddressOf())));

    // Create descriptor to mip level 0 of entire resource using the format of the resource.
    md3dDevice->CreateDepthStencilView(mDepthStencilBuffer.Get(), nullptr, depthStencilView());

    // Transition the resource from its initial state to be used as a depth buffer.
	auto transition = CD3DX12_RESOURCE_BARRIER::Transition(mDepthStencilBuffer.Get(),
		D3D12_RESOURCE_STATE_COMMON, D3D12_RESOURCE_STATE_DEPTH_WRITE);
	mCommandList->ResourceBarrier(1, &transition);
	
    // Execute the resize commands.
    D3DUtil::throwIfFailed(mCommandList->Close());
    ID3D12CommandList* cmdsLists[] = { mCommandList.Get() };
    mCommandQueue->ExecuteCommandLists(_countof(cmdsLists), cmdsLists);

	// Wait until resize is complete.
	flushCommandQueue();

	// Update the viewport transform to cover the client area.
	mScreenViewport.TopLeftX = 0;
	mScreenViewport.TopLeftY = 0;
	mScreenViewport.Width    = static_cast<float>(mClientWidth);
	mScreenViewport.Height   = static_cast<float>(mClientHeight);
	mScreenViewport.MinDepth = 0.0f;
	mScreenViewport.MaxDepth = 1.0f;

    mScissorRect = {0, 0, mClientWidth, mClientHeight};
}
 
LRESULT D3DApp::processMessage(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	// WM_ACTIVATE is sent when the window is activated or deactivated.  
	// We pause the game when the window is deactivated and unpause it 
	// when it becomes active.  
	case WM_ACTIVATE:
		if (LOWORD(wParam) == WA_INACTIVE)
		{
			mAppPaused = true;
			mTimer.Stop();
		}
		else
		{
			mAppPaused = false;
			mTimer.Start();
		}
		return 0;

	// WM_SIZE is sent when the user resizes the window.  
	case WM_SIZE:
		// Save the new client area dimensions.
		mClientWidth  = LOWORD(lParam);
		mClientHeight = HIWORD(lParam);
		if (md3dDevice)
		{
			if (wParam == SIZE_MINIMIZED)
			{
				mAppPaused = true;
				mMinimized = true;
				mMaximized = false;
			}
			else if (wParam == SIZE_MAXIMIZED)
			{
				mAppPaused = false;
				mMinimized = false;
				mMaximized = true;
				onResize();
			}
			else if (wParam == SIZE_RESTORED)
			{
				// Restoring from minimized state?
				if (mMinimized)
				{
					mAppPaused = false;
					mMinimized = false;
					onResize();
				}
				// Restoring from maximized state?
				else if (mMaximized)
				{
					mAppPaused = false;
					mMaximized = false;
					onResize();
				}
				else if (mResizing)
				{
					// If user is dragging the resize bars, we do not resize 
					// the buffers here because as the user continuously 
					// drags the resize bars, a stream of WM_SIZE messages are
					// sent to the window, and it would be pointless (and slow)
					// to resize for each WM_SIZE message received from dragging
					// the resize bars.  So instead, we reset after the user is 
					// done resizing the window and releases the resize bars, which 
					// sends a WM_EXITSIZEMOVE message.
				}
				else // API call such as SetWindowPos or mSwapChain->SetFullscreenState.
				{
					onResize();
				}
			}
		}
		return 0;

	// WM_EXITSIZEMOVE is sent when the user grabs the resize bars.
	case WM_ENTERSIZEMOVE:
		mAppPaused = true;
		mResizing  = true;
		mTimer.Stop();
		return 0;

	// WM_EXITSIZEMOVE is sent when the user releases the resize bars.
	// Here we reset everything based on the new window dimensions.
	case WM_EXITSIZEMOVE:
		mAppPaused = false;
		mResizing  = false;
		mTimer.Start();
		onResize();
		return 0;
 
	// WM_DESTROY is sent when the window is being destroyed.
	case WM_DESTROY:
		PostQuitMessage(0);
		return 0;

	// The WM_MENUCHAR message is sent when a menu is active and the user presses 
	// a key that does not correspond to any mnemonic or accelerator key. 
	case WM_MENUCHAR:
        // Don't beep when we alt-enter.
        return MAKELRESULT(0, MNC_CLOSE);

	// Catch this message so to prevent the window from becoming too small.
	case WM_GETMINMAXINFO:
		((MINMAXINFO*)lParam)->ptMinTrackSize.x = 200;
		((MINMAXINFO*)lParam)->ptMinTrackSize.y = 200; 
		return 0;

	// Mouse commands
	case WM_LBUTTONDOWN:
	case WM_MBUTTONDOWN:
	case WM_RBUTTONDOWN:
		onMouseDown(wParam, GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
		return 0;
	case WM_LBUTTONUP:
	case WM_MBUTTONUP:
	case WM_RBUTTONUP:
		onMouseUp(wParam, GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
		return 0;
	case WM_MOUSEMOVE:
		onMouseMove(wParam, GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
		return 0;

	// Keyboard commands - only listen for key up of escape key
    case WM_KEYUP:
        if (wParam == VK_ESCAPE)
        {
            PostQuitMessage(0);
        }
        else if (static_cast<int>(wParam) == VK_F2)
            set4xMsaaState(!m4xMsaaState);

        return 0;
	}

	return DefWindowProc(hwnd, msg, wParam, lParam);
}

bool D3DApp::initMainWindow()
{
	WNDCLASS wc
	{
		CS_HREDRAW | CS_VREDRAW, // style
		mainWindowsProcedure,             // lpfnWndProc
		0,                       // cbClsExtra
		0,                       // cbWndExtra
		mhAppInst,               // hInstance
		LoadIcon(0, IDI_APPLICATION), // hIcon
		LoadCursor(0, IDC_ARROW),     // hCursor
		(HBRUSH)GetStockObject(NULL_BRUSH), // hbrBackground
		0,                       // lpszMenuName
		L"MainWnd"               // lpszClassName
	};

	if(!RegisterClass(&wc))
	{
		MessageBox(0, L"RegisterClass Failed.", 0, 0);
		return false;
	}

	// Compute window rectangle dimensions based on requested client area dimensions.
	RECT R = {0, 0, mClientWidth, mClientHeight};
    AdjustWindowRect(&R, WS_OVERLAPPEDWINDOW, false);
	int width  = R.right - R.left;
	int height = R.bottom - R.top;

	mhMainWnd = CreateWindow(L"MainWnd", mMainWndCaption.c_str(), 
		WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, CW_USEDEFAULT, width, height, 0, 0, mhAppInst, 0); 
	if( !mhMainWnd )
	{
		MessageBox(0, L"CreateWindow Failed.", 0, 0);
		return false;
	}

	ShowWindow(mhMainWnd, SW_SHOW);
	UpdateWindow(mhMainWnd);

	return true;
}

bool D3DApp::initDirect3D()
{
//#if defined(DEBUG) || defined(_DEBUG) 
//	// Enable the D3D12 debug layer.
//{
//	ComPtr<ID3D12Debug> debugController;
//	D3DUtil::throwIfFailed(D3D12GetDebugInterface(IID_PPV_ARGS(&debugController)));
//	debugController->EnableDebugLayer();
//}
//#endif

	D3DUtil::throwIfFailed(CreateDXGIFactory1(IID_PPV_ARGS(&mdxgiFactory)));
	// Try to create hardware device.
	HRESULT hardwareResult = D3D12CreateDevice(
		nullptr,             // default adapter
		D3D_FEATURE_LEVEL_11_0,
		IID_PPV_ARGS(&md3dDevice));

	// Fallback to WARP device.
	if(FAILED(hardwareResult))
	{
		ComPtr<IDXGIAdapter> pWarpAdapter;
		D3DUtil::throwIfFailed(mdxgiFactory->EnumWarpAdapter(IID_PPV_ARGS(&pWarpAdapter)));

		D3DUtil::throwIfFailed(D3D12CreateDevice(
			pWarpAdapter.Get(),
			D3D_FEATURE_LEVEL_11_0,
			IID_PPV_ARGS(&md3dDevice)));
	}
	//MessageBox(nullptr, L"432", L"DirectX error!", MB_OK);

	D3DUtil::throwIfFailed(md3dDevice->CreateFence(0, D3D12_FENCE_FLAG_NONE,
		IID_PPV_ARGS(&mFence)));

	mRtvDescriptorSize = md3dDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);
	mDsvDescriptorSize = md3dDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_DSV);
	mCbvSrvUavDescriptorSize = md3dDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
	//MessageBox(nullptr, L"440", L"DirectX error!", MB_OK);
    // Check 4X MSAA quality support for our back buffer format.
    // All Direct3D 11 capable devices support 4X MSAA for all render 
    // target formats, so we only need to check quality support.

	D3D12_FEATURE_DATA_MULTISAMPLE_QUALITY_LEVELS msQualityLevels
	{
		mBackBufferFormat, // Format
		4,                 // SampleCount
		D3D12_MULTISAMPLE_QUALITY_LEVELS_FLAG_NONE, // Flags
		0                  // NumQualityLevels
	};

	D3DUtil::throwIfFailed(md3dDevice->CheckFeatureSupport(
		D3D12_FEATURE_MULTISAMPLE_QUALITY_LEVELS,
		&msQualityLevels,
		sizeof(msQualityLevels)));
	//MessageBox(nullptr, L"454", L"DirectX error!", MB_OK);
    m4xMsaaQuality = msQualityLevels.NumQualityLevels;
	assert(m4xMsaaQuality > 0 && "Unexpected MSAA quality level.");
	
	createCommandObjects();
    createSwapChain();
    createRtvAndDsvDescriptorHeaps();
	return true;
}

void D3DApp::createCommandObjects()
{
	D3D12_COMMAND_QUEUE_DESC queueDesc
	{
		D3D12_COMMAND_LIST_TYPE_DIRECT, // Type
		0,                              // Priority
		D3D12_COMMAND_QUEUE_FLAG_NONE,  // Flags
		0                               // NodeMask
	};

	D3DUtil::throwIfFailed(md3dDevice->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&mCommandQueue)));

	D3DUtil::throwIfFailed(md3dDevice->CreateCommandAllocator(
		D3D12_COMMAND_LIST_TYPE_DIRECT,
		IID_PPV_ARGS(mDirectCmdListAlloc.GetAddressOf())));

	D3DUtil::throwIfFailed(md3dDevice->CreateCommandList(
		0,
		D3D12_COMMAND_LIST_TYPE_DIRECT,
		mDirectCmdListAlloc.Get(), // Associated command allocator
		nullptr,                   // Initial PipelineStateObject
		IID_PPV_ARGS(mCommandList.GetAddressOf())));

	// Start off in a closed state.  This is because the first time we refer 
	// to the command list we will Reset it, and it needs to be closed before
	// calling Reset.
	mCommandList->Close();
}

void D3DApp::createSwapChain()
{
    // Release the previous swapchain we will be recreating.
    mSwapChain.Reset();

    DXGI_SWAP_CHAIN_DESC sd
	{
		{
			static_cast<UINT>(mClientWidth),   // Width
			static_cast<UINT>(mClientHeight),   // Height
			{60, 1},        // RefreshRate
			mBackBufferFormat, // Format
			DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED, // ScanlineOrdering
			DXGI_MODE_SCALING_UNSPECIFIED  // Scaling
		}, // BufferDesc
		{
			m4xMsaaState ? 4u : 1u, // Count
			m4xMsaaState ? (static_cast<UINT>(m4xMsaaQuality - 1)) : 0  // Quality
		}, // SampleDesc
		DXGI_USAGE_RENDER_TARGET_OUTPUT, // BufferUsage
		swapChainBufferCount,            // BufferCount
		mhMainWnd,                     // OutputWindow
		true,                            // Windowed
		DXGI_SWAP_EFFECT_FLIP_DISCARD,  // SwapEffect
		DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH // Flags
	};

	mCommandQueue.Get();
	
	mSwapChain.GetAddressOf();

	// Note: Swap chain uses queue to perform flush.
    D3DUtil::throwIfFailed(mdxgiFactory->CreateSwapChain(
		mCommandQueue.Get(),
		&sd, 
		mSwapChain.GetAddressOf()));
}

void D3DApp::flushCommandQueue()
{
	// Advance the fence value to mark commands up to this fence point.
    ++mCurrentFence;

    // Add an instruction to the command queue to set a new fence point.  Because we 
	// are on the GPU timeline, the new fence point won't be set until the GPU finishes
	// processing all the commands prior to this Signal().
    D3DUtil::throwIfFailed(mCommandQueue->Signal(mFence.Get(), mCurrentFence));

	// Wait until the GPU has completed commands up to this fence point.
    if(mFence->GetCompletedValue() < mCurrentFence)
	{
		HANDLE eventHandle = CreateEventEx(nullptr, nullptr, false, EVENT_ALL_ACCESS);

        // Fire event when GPU hits current fence.  
        D3DUtil::throwIfFailed(mFence->SetEventOnCompletion(mCurrentFence, eventHandle));

        // Wait until the GPU hits current fence event is fired.
		if (eventHandle != 0)
		{
			WaitForSingleObject(eventHandle, INFINITE);
			CloseHandle(eventHandle);
		}
	}
}

ID3D12Resource* D3DApp::currentBackBuffer()const
{
	return mSwapChainBuffer[mCurrBackBuffer].Get();
}

D3D12_CPU_DESCRIPTOR_HANDLE D3DApp::currentBackBufferView()const
{
	return CD3DX12_CPU_DESCRIPTOR_HANDLE(
		mRtvHeap->GetCPUDescriptorHandleForHeapStart(),
		mCurrBackBuffer,
		mRtvDescriptorSize);
}

D3D12_CPU_DESCRIPTOR_HANDLE D3DApp::depthStencilView()const
{
	return mDsvHeap->GetCPUDescriptorHandleForHeapStart();
}