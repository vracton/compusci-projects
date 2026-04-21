#pragma once

#include <windows.h>

#include "ArenaDisplay.h"
#include "GraphicRegistry.h"

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

class CoreDrawingDLL
{
public:
	CoreDrawingDLL();
	~CoreDrawingDLL();
	bool WindowRegister(LPCWSTR ClassName);
	HWND WindowMake(LPCWSTR ClassName, DWORD style, int height, int width, HWND parent);

	void addGraphic(LPCWSTR file, double width, double height, int index);

	void addVisualLayer();

	void addGraphicInstance(int layerIndex, int graphicIndex, int index,
		Coordinate<double> location);

	void moveGraphic(int layerIndex, int index, Coordinate<double> location);
	void rotateGraphic(int layerIndex, int index, double angle);
	void removeGraphicInstance(int layerIndex, int index);
	void changeGraphic(int layerIndex, int index, int newGraphicIndex);

	void redraw();

	void onResize(UINT width, UINT height);

	void zoom(double widthFactor, double heightFactor, double centerX, double centerY);
	void translate(double xShift, double yShift);

	void reset();
	/*void RunMessageLoop();*/

private:
	HRESULT CreateDeviceResources();
	void DiscardDeviceResources();

	HRESULT OnRender();

	static LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);

	HWND hwnd;
	ID2D1Factory* pDirect2dFactory;
	ID2D1HwndRenderTarget* pRenderTarget;
	IWICImagingFactory* pImagingFactory;
	GraphicRegistry registry;
	ArenaDisplay arena;
};