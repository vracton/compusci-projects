//***************************************************************************************
// d3dApp.h by Frank Luna (C) 2015 All Rights Reserved.
// Modified by Peter Dong
//***************************************************************************************

#pragma once

#if defined(DEBUG) || defined(_DEBUG)
#define _CRTDBG_MAP_ALLOC
#include <crtdbg.h>
#endif

#include "D3DUtil.h"
#include "GameTimer.h"
#include <mutex>

// Link necessary d3d12 libraries.
#pragma comment(lib,"d3dcompiler.lib")
#pragma comment(lib, "D3D12.lib")
#pragma comment(lib, "dxgi.lib")

// A generic 3D app that uses DirectX
// As an abstract base class
// Should be a singleton
class D3DApp
{
public:
	// A version of itself that is accessible statically, updated whenever the 
	// constructor is called
	static D3DApp* getApp();

	// The handle to the instance of the app
	HINSTANCE appInstance() const;

	// The handle to the main window
	HWND mainWindow() const;

	// The client aspect ratio
	float aspectRatio() const;

	// This is for Multi-Sampling Auto-Aliasing
	bool get4xMsaaState() const;
	void set4xMsaaState(bool value);

	// The main method to run the app, including the main message loop
	int run();

	// Intializes the display
	virtual bool initialize();

	// The window message processing function
	virtual LRESULT processMessage(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

	void setPaused(bool value) { mAppPaused = value; }

protected:
	D3DApp(HINSTANCE hInstance);
	// Do not allow copying or assignment
	D3DApp(const D3DApp& rhs) = delete;
	D3DApp& operator=(const D3DApp& rhs) = delete;
	virtual ~D3DApp();

	// Set up descriptor heaps for Direct3D
	virtual void createRtvAndDsvDescriptorHeaps();
	// What to do on a resize
	virtual void onResize();
	// The main window procedure function
	virtual void update(const GameTimer& gt) = 0;
	// Draw the scene
	virtual void draw(const GameTimer& gt) = 0;

	// Runs the internal state machine that handles the main loop
	virtual void runStateMachine(const GameTimer& gt) = 0;

	// Convenience overrides for handling mouse input.
	virtual void onMouseDown(WPARAM btnState, int x, int y) {}
	virtual void onMouseUp(WPARAM btnState, int x, int y) {}
	virtual void onMouseMove(WPARAM btnState, int x, int y) {}

	// DirectX initialization methods
	bool initMainWindow();
	bool initDirect3D();
	void createCommandObjects();
	void createSwapChain();

	// Flush the command queue to make sure that the GPU is done processing commands before we reset
	void flushCommandQueue();

	// Accessors to buffers and views
	ID3D12Resource* currentBackBuffer() const;
	D3D12_CPU_DESCRIPTOR_HANDLE currentBackBufferView() const;
	D3D12_CPU_DESCRIPTOR_HANDLE depthStencilView() const;

	// Main Windows procedure function
	// As a static function because it can be called before initialization, but it just forwards to the app's version
	static LRESULT mainWindowsProcedure(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

	void setMainWindowHandle(HWND hWnd) { mhMainWnd = hWnd; }
	void setApplicationInstance(HINSTANCE hInstance) { mhAppInst = hInstance; }

	// Accessors to members for derived classes
	Microsoft::WRL::ComPtr <IDXGISwapChain> getSwapChain() const { return mSwapChain; }
	Microsoft::WRL::ComPtr<ID3D12Device> getD3DDevice() const { return md3dDevice; }
	Microsoft::WRL::ComPtr<ID3D12Fence> getFence() const { return mFence; }
	Microsoft::WRL::ComPtr<ID3D12CommandQueue> getCommandQueue() const { return mCommandQueue; }
	Microsoft::WRL::ComPtr<ID3D12CommandAllocator> getCommandAllocator() const { return mDirectCmdListAlloc; }
	Microsoft::WRL::ComPtr<ID3D12GraphicsCommandList> getCommandList() const { return mCommandList; }
	
	int getCurrentBackBufferIndex() const { return mCurrBackBuffer; }
	void setCurrentBackBufferIndex(int index) { mCurrBackBuffer = index; }
	int getSwapChainBufferCount() const { return swapChainBufferCount; }
	const DXGI_FORMAT& getBackBufferFormat() const{ return mBackBufferFormat; }
	const DXGI_FORMAT& getDepthStencilFormat() const { return mDepthStencilFormat; }

	UINT get4xMsaaQuality() const { return m4xMsaaQuality; }

	UINT64 getCurrentFence() const { return mCurrentFence; }
	void setCurrentFence(UINT64 value) { mCurrentFence = value; }
	void incrementCurrentFence() { ++mCurrentFence; }

	const D3D12_VIEWPORT& getScreenViewport() const { return mScreenViewport; }
	const D3D12_RECT& getScissorRect() const { return mScissorRect; }

	const std::atomic<bool>& getChangesNeeded() const { return changesNeeded; }
	const std::atomic<bool>& getDoneDrawing() const { return doneDrawing; }
	void setDoneDrawing(bool value) { doneDrawing = value; }

	bool isPaused() const { return mAppPaused; }
	int getClientWidth() const { return mClientWidth; }
	int getClientHeight() const { return mClientHeight; }

private:
	// The main app object, run as a singleton
	static D3DApp* mApp;

	HINSTANCE mhAppInst = nullptr; // application instance handle
	HWND      mhMainWnd = nullptr; // main window handle
	bool      mAppPaused = false;  // is the application paused?
	bool      mMinimized = false;  // is the application minimized?
	bool      mMaximized = false;  // is the application maximized?
	bool      mResizing = false;   // are the resize bars being dragged?
	bool      mFullscreenState = false;// fullscreen enabled

	// Set true to use 4X MSAA (§4.1.8).  The default is false.
	bool      m4xMsaaState = false;    // 4X MSAA enabled
	UINT      m4xMsaaQuality = 0;      // quality level of 4X MSAA

	// Used to keep track of the “delta-time” and game time (§4.4).
	GameTimer mTimer;

	Microsoft::WRL::ComPtr<IDXGIFactory4> mdxgiFactory;
	Microsoft::WRL::ComPtr<IDXGISwapChain> mSwapChain;
	Microsoft::WRL::ComPtr<ID3D12Device> md3dDevice;

	Microsoft::WRL::ComPtr<ID3D12Fence> mFence;
	UINT64 mCurrentFence = 0;

	Microsoft::WRL::ComPtr<ID3D12CommandQueue> mCommandQueue;
	Microsoft::WRL::ComPtr<ID3D12CommandAllocator> mDirectCmdListAlloc;
	Microsoft::WRL::ComPtr<ID3D12GraphicsCommandList> mCommandList;

	static const int swapChainBufferCount = 2;
	int mCurrBackBuffer = 0;
	Microsoft::WRL::ComPtr<ID3D12Resource> mSwapChainBuffer[swapChainBufferCount];
	Microsoft::WRL::ComPtr<ID3D12Resource> mDepthStencilBuffer;

	Microsoft::WRL::ComPtr<ID3D12DescriptorHeap> mRtvHeap;
	Microsoft::WRL::ComPtr<ID3D12DescriptorHeap> mDsvHeap;

	D3D12_VIEWPORT mScreenViewport = { 0, 0, 0, 0, 0, 0 };
	D3D12_RECT mScissorRect = { 0, 0, 0, 0 };

	std::atomic<bool> changesNeeded = false;
	std::atomic<bool> doneDrawing = true;

	UINT mRtvDescriptorSize = 0;
	UINT mDsvDescriptorSize = 0;
	UINT mCbvSrvUavDescriptorSize = 0;

	// Derived class should set these in derived constructor to customize starting values.
	std::wstring mMainWndCaption = L"d3d App";
	D3D_DRIVER_TYPE md3dDriverType = D3D_DRIVER_TYPE_HARDWARE;
	DXGI_FORMAT mBackBufferFormat = DXGI_FORMAT_R8G8B8A8_UNORM;
	DXGI_FORMAT mDepthStencilFormat = DXGI_FORMAT_D24_UNORM_S8_UINT;
	int mClientWidth = 800;
	int mClientHeight = 600;
};

