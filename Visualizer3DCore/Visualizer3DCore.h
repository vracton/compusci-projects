//***************************************************************************************
// LitColumnsApp.cpp by Frank Luna (C) 2015 All Rights Reserved.
//***************************************************************************************

#include "D3DApp.h"
#include "MathHelper.h"
#include "UploadBuffer.h"
#include "GeometryGenerator.h"
#include "FrameResource.h"
#include "Camera.h"
#include "VisualizerCommand.h"
#include <queue>

using Microsoft::WRL::ComPtr;
using namespace DirectX;
using namespace DirectX::PackedVector;

#pragma comment(lib, "d3dcompiler.lib")
#pragma comment(lib, "D3D12.lib")

#ifndef Assert
#if defined( DEBUG ) || defined( _DEBUG )
#define Assert(b) do {if (!(b)) {OutputDebugStringA("Assert: " #b "\n");}} while(0)
#else
#define Assert(b)
#endif //DEBUG || _DEBUG
#endif

#ifndef HINST_THISCOMPONENT
EXTERN_C IMAGE_DOS_HEADER __ImageBase;
#define HINST_THISCOMPONENT ((HINSTANCE)&__ImageBase)
#endif

class Visualizer3DCore : public D3DApp
{
public:
    Visualizer3DCore(HINSTANCE hInstance);
    Visualizer3DCore(const Visualizer3DCore& rhs) = delete;
    Visualizer3DCore& operator=(const Visualizer3DCore& rhs) = delete;
    ~Visualizer3DCore();

    virtual bool initialize() override;
    void ReInitialize();

    // Methods to add geometry objects
    void addMeshData(int index, const GeometryGenerator::MeshData& mesh);
    void addMaterial(int index, DirectX::XMVECTORF32 color,
        float fresnel, float roughness);
    void addMaterial(int index, float colorR, float colorG, float colorB,
        float alpha, float fresnel, float roughness);
    void changeMaterialColor(int index, XMVECTORF32 color);
    void addObject(int index, DirectX::XMFLOAT3 scale, DirectX::XMFLOAT3X3 rotation,
        DirectX::XMFLOAT3 position, int materialIndex, int shapeIndex);
	DirectX::XMFLOAT3 getObjectPosition(int index) const;
    void moveObject(int index, DirectX::XMFLOAT3 newPosition);
    void transformObject(int index, DirectX::XMFLOAT3 newScale, DirectX::XMFLOAT3X3 newRotation,
        DirectX::XMFLOAT3 newPosition);
    void removeObject(int index);
    void removeAllObjects();
    void clearAll();

    void setAutoCamera(bool value) { autoCamera = value; }
    void moveCamera(DirectX::XMFLOAT3 newPosition);
    void moveCamera(DirectX::XMFLOAT3 newPosition, DirectX::XMFLOAT3 lookDirection, DirectX::XMFLOAT3 upDirection);
    void lookAt(DirectX::XMFLOAT3 newPosition, DirectX::XMFLOAT3 target, DirectX::XMFLOAT3 upDirection);
    void adjustCameraLens(float fieldOfViewY, float aspectRatio, float nearZ, float farZ);
    void singleAutoCameraAdjustment();
    const Camera& getCamera() const { return camera; }

    //bool currentlyDrawing() const { return alreadyInitialized && !mAppPaused; }
    void doCommand(const std::shared_ptr<VisualizerCommand> command);

    // For the DLL
    bool WindowRegister(LPCWSTR ClassName);
    HWND WindowMake(LPCWSTR ClassName, DWORD style, int height, int width, HWND parent);

    // Converts a three-element array into an XMFLOAT3 structure
    static XMFLOAT3 ConvertToXMFLOAT3(float array[]);

    enum class State {Initializing, ReadyToBuildMeshes, ReadyToBuildObjects, Paused, ReadyToDraw, ReadyToUpdate};
    State getState() const { return currentState; }

private:
    // Lightweight structure stores parameters to draw a shape.  This will
// vary from app-to-app.
    struct RenderItem
    {
        RenderItem() = default;

        // World matrix of the shape that describes the object's local space
        // relative to the world space, which defines the position, orientation,
        // and scale of the object in the world.
        XMFLOAT4X4 World = MathHelper::Identity4x4();

        XMFLOAT4X4 TexTransform = MathHelper::Identity4x4();

        // Dirty flag indicating the object data has changed and we need to update the constant buffer.
        // Because we have an object cbuffer for each FrameResource, we have to apply the
        // update to each FrameResource.  Thus, when we modify obect data we should set 
        // NumFramesDirty = gNumFrameResources so that each frame resource gets the update.
        int NumFramesDirty = D3DUtil::getNFrameResources();

        // Index into GPU constant buffer corresponding to the ObjectCB for this render item.
        UINT ObjCBIndex = -1;

        Material* Mat = nullptr;
        MeshGeometry* Geo = nullptr;

        // Primitive topology.
        D3D12_PRIMITIVE_TOPOLOGY PrimitiveType = D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST;

        // DrawIndexedInstanced parameters.
        UINT IndexCount = 0;
        UINT StartIndexLocation = 0;
        int BaseVertexLocation = 0;

        bool isOpaque() const { return Mat->IsOpaque(); }
        // Where to find this in either the transparent or opaque lists
        int underlyingLayerIndex = -1;

    };

    virtual void onResize() override;
    virtual void update(const GameTimer& gt) override;
    virtual void draw(const GameTimer& gt) override;
    virtual void runStateMachine(const GameTimer& gt) override;

    virtual void onMouseDown(WPARAM btnState, int x, int y) override;
    virtual void onMouseUp(WPARAM btnState, int x, int y) override;
    virtual void onMouseMove(WPARAM btnState, int x, int y) override;

    void OnKeyboardInput(const GameTimer& gt);
    void UpdateCamera(const GameTimer& gt);
    void AnimateMaterials(const GameTimer& gt);
    void UpdateObjectCBs(const GameTimer& gt);
    void UpdateMaterialCBs(const GameTimer& gt);
    void UpdateMainPassCB(const GameTimer& gt);

    void BuildRootSignature();
    void BuildShadersAndInputLayout();
    void BuildShapeGeometry();
    void BuildPSOs();
    void BuildFrameResources();
    void BuildMaterials();
    void BuildRenderItems();
    void DrawRenderItems(ID3D12GraphicsCommandList* cmdList, const std::vector<RenderItem*>& ritems);

    // State changes
    void InitializeDirectX();
    void BuildMeshes();
    void BuildObjects();
    void Unpause();
    void ProcessCommandQueue();
    State checkForNextState() const;

    float getLargestDistanceFromCenter() const;
    void calculateCenter();
    void rescaleAll();
    void rescaleZ();
    void autoCameraAdjust();

    void MarkAllDirty();

    // Containers to hold geometry objects
    std::vector<GeometryGenerator::MeshData> meshes;
    struct ObjectInfo
    {
        ObjectInfo(DirectX::XMFLOAT3 scaleVec, XMFLOAT3X3 rotationVec,
            DirectX::XMFLOAT3 translationVec, int material, int shape);

        DirectX::XMFLOAT3 scale;
        DirectX::XMFLOAT3 translation;
        DirectX::XMMATRIX rotation;
        int materialIndex;
        int shapeIndex;

        static float globalScaleFactor;

        void fillRenderItem(RenderItem* renderItem) const;
        void updateTransformation(DirectX::XMFLOAT3 scaleVec, DirectX::XMFLOAT3X3 rotationVec,
            DirectX::XMFLOAT3 translationVec);
        void updateTranslation(DirectX::XMFLOAT3 translationVec) { translation = translationVec; }
        void getMatrix(XMMATRIX& result) const;

    };


    void BuildOneRenderItem(ObjectInfo info);

    std::vector<ObjectInfo> objects;

    std::vector<std::unique_ptr<FrameResource>> mFrameResources;
    FrameResource* mCurrFrameResource = nullptr;
    int mCurrFrameResourceIndex = 0;

    UINT mCbvSrvDescriptorSize = 0;

    ComPtr<ID3D12RootSignature> mRootSignature = nullptr;

    ComPtr<ID3D12DescriptorHeap> mSrvDescriptorHeap = nullptr;

    std::unique_ptr<MeshGeometry> mGeometries;
    std::vector<std::unique_ptr<Material>> mMaterials;
    //std::unordered_map<std::string, std::unique_ptr<Texture>> mTextures;
    std::unordered_map<std::string, ComPtr<ID3DBlob>> mShaders;
    ComPtr<ID3D12PipelineState> mOpaquePSO;
    ComPtr<ID3D12PipelineState> mTransparentPSO;

    std::vector<D3D12_INPUT_ELEMENT_DESC> mInputLayout;

    // List of all the render items.
    std::vector<std::unique_ptr<RenderItem>> mAllRitems;

    // Render items divided by PSO.
    std::vector<RenderItem*> mOpaqueItems;
    std::vector<RenderItem*> mTransparentItems;

    PassConstants mMainPassCB;

    Camera camera;
    XMFLOAT3 cameraVel = XMFLOAT3(0, 0, 0);
    float cameraAccel = 100;
    float cameraDrag = 500;
    float maxSpeed = 100;
    const float cameraRotation = .0005f;

    // Store these vertices for use by auto-camera
    std::vector<std::vector<Vertex>> storedVertices;
    bool autoCamera = false;
    bool oneTimeAutoCamera = true; // Set to true to guarantee an auto camera on the first frame

    XMVECTOR center;

    bool alreadyInitialized = false;

    int rItemsCapacity = 0;
    int materialsCapacity = 0;
    int indexCounter = 0;

    State currentState = State::Initializing;
    std::atomic<bool> currentlyExecuting = false;
    bool meshesNeedUpdating = true;
    bool objectsNeedRebuilding = true;

	std::unordered_map<int, int> objectIndexMap; // Maps from user shape indices to internal shape indices

    std::mutex queueLock;

    std::deque<std::shared_ptr<VisualizerCommand>> commandQueue;

    POINT mLastMousePos;

    HWND hwnd;
    static LRESULT CALLBACK ProcProxy(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
};
