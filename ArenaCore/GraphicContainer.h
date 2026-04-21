#pragma once

#include <string_view>

#include "d2d1.h"
#include "wincodec.h"

#pragma comment(lib, "d2d1.lib")

class GraphicContainer
{
public:
	GraphicContainer(LPCWSTR filename, ID2D1HwndRenderTarget* renderTarget, 
		IWICImagingFactory* imagingFactory, double width, double height);
	~GraphicContainer();

	ID2D1BitmapBrush* getBrush() const { return brush; }
	const D2D1::Matrix3x2F& getScale() const { return scalingMatrix; }
	void scaleToSize(double width, double height);
	void draw(int x, int y, ID2D1HwndRenderTarget* renderTarget, double rotation = 0);
	void rescale(D2D1_SIZE_U oldSize, ID2D1HwndRenderTarget* newRenderTarget);

private:
	static ID2D1BitmapBrush* createBrush(LPCWSTR filename, 
		ID2D1HwndRenderTarget* renderTarget, IWICImagingFactory* imagingFactory);

	static HRESULT loadBitmapFromFile(ID2D1RenderTarget *pRenderTarget,
		IWICImagingFactory *pIWICFactory, PCWSTR uri, ID2D1Bitmap **ppBitmap);

	static D2D1_SIZE_U calcSize(ID2D1BitmapBrush* brush);

	void setScaleMatrix(D2D1::Matrix3x2F matrix);

	ID2D1BitmapBrush* brush;
	D2D1::Matrix3x2F scalingMatrix;
	D2D1_RECT_F rectangle;
};

