#include "stdafx.h"
#include "GraphicContainer.h"

#include <sstream>

GraphicContainer::GraphicContainer(LPCWSTR filename, 
	ID2D1HwndRenderTarget* renderTarget, IWICImagingFactory* imagingFactory, 
	double width, double height) :
	brush(createBrush(filename, renderTarget, imagingFactory)),
	scalingMatrix(D2D1::Matrix3x2F::Identity()),
	rectangle(D2D1::RectF(0, 0, 0, 0))
{
	auto size = renderTarget->GetPixelSize();
	scaleToSize(width * size.width, height * size.height);
}


GraphicContainer::~GraphicContainer()
{
	SafeRelease(&brush);
}

void GraphicContainer::scaleToSize(double width, double height)
{
	brush->SetTransform(D2D1::Matrix3x2F::Identity());
	auto size = calcSize(brush);

	auto scale = D2D1::Matrix3x2F::Scale(
		D2D1::Size(static_cast<float>(width) / size.width,
			static_cast<float>(height) / size.height),
		D2D1::Point2F(0, 0));
	auto translate = D2D1::Matrix3x2F::Translation(static_cast<float>(-width / 2), static_cast<float>(-height / 2));
	setScaleMatrix(scale * translate);

	rectangle = D2D1::RectF(0, 0, static_cast<FLOAT>(width), static_cast<FLOAT>(height));
}

void GraphicContainer::draw(int x, int y, ID2D1HwndRenderTarget* renderTarget, double rotation)
{
	auto translateMatrix = D2D1::Matrix3x2F::Translation(x, y);
	auto rotationMatrix = D2D1::Matrix3x2F::Rotation(static_cast<float>(rotation), D2D1::Point2F(0, 0));
	brush->SetTransform(scalingMatrix * rotationMatrix * translateMatrix);

	float width = rectangle.right - rectangle.left;
	float height = rectangle.bottom - rectangle.top;

	D2D1::Matrix3x2F matrix;
	brush->GetTransform(&matrix);

	auto rect = D2D1::RectF(
		x - width / 2,
		y - height / 2,
		x + width / 2,
		y + height / 2
	);

	renderTarget->FillRectangle(rect, brush);
}

ID2D1BitmapBrush* GraphicContainer::createBrush(LPCWSTR filename, 
	ID2D1HwndRenderTarget* renderTarget, IWICImagingFactory* imagingFactory)
{
	ID2D1Bitmap* m_pBitmap = NULL;
	HRESULT hr = loadBitmapFromFile(
		renderTarget,
		imagingFactory,
		filename,
		&m_pBitmap
	);

	ID2D1BitmapBrush* m_pBitmapBrush = NULL;

	if (SUCCEEDED(hr))
	{
		hr = renderTarget->CreateBitmapBrush(
			m_pBitmap,
			&m_pBitmapBrush
		);
	}

	return m_pBitmapBrush;
}

HRESULT GraphicContainer::loadBitmapFromFile(ID2D1RenderTarget *pRenderTarget,
	IWICImagingFactory *pIWICFactory,
	PCWSTR uri,
	ID2D1Bitmap **ppBitmap)
{
	IWICBitmapDecoder *pDecoder = NULL;
	IWICBitmapFrameDecode *pSource = NULL;
	IWICStream *pStream = NULL;
	IWICFormatConverter *pConverter = NULL;
	IWICBitmapScaler *pScaler = NULL;

	HRESULT hr = pIWICFactory->CreateDecoderFromFilename(
		uri,
		NULL,
		GENERIC_READ,
		WICDecodeMetadataCacheOnLoad,
		&pDecoder
	);


	if (SUCCEEDED(hr))
	{
		// Create the initial frame.
		hr = pDecoder->GetFrame(0, &pSource);
	}

	if (SUCCEEDED(hr))
	{
		// Convert the image format to 32bppPBGRA
		// (DXGI_FORMAT_B8G8R8A8_UNORM + D2D1_ALPHA_MODE_PREMULTIPLIED).
		hr = pIWICFactory->CreateFormatConverter(&pConverter);

	}


	if (SUCCEEDED(hr))
	{
		hr = pConverter->Initialize(
			pSource,
			GUID_WICPixelFormat32bppPBGRA,
			WICBitmapDitherTypeNone,
			NULL,
			0.f,
			WICBitmapPaletteTypeMedianCut
		);
	}

	if (SUCCEEDED(hr))
	{
		// Create a Direct2D bitmap from the WIC bitmap.
		hr = pRenderTarget->CreateBitmapFromWicBitmap(
			pConverter,
			NULL,
			ppBitmap
		);
	}

	SafeRelease(&pDecoder);
	SafeRelease(&pSource);
	SafeRelease(&pStream);
	SafeRelease(&pConverter);
	SafeRelease(&pScaler);

	return hr;
}

D2D1_SIZE_U GraphicContainer::calcSize(ID2D1BitmapBrush * brush)
{
	ID2D1Bitmap* bitmap;
	brush->GetBitmap(&bitmap);
	return bitmap->GetPixelSize();
}

void GraphicContainer::setScaleMatrix(D2D1::Matrix3x2F matrix)
{
	scalingMatrix = matrix;
	//brush->SetTransform(scalingMatrix);
}

void GraphicContainer::rescale(D2D1_SIZE_U oldSize, ID2D1HwndRenderTarget* newRenderTarget)
{
	auto newsize = newRenderTarget->GetPixelSize();

	auto xSize = rectangle.right - rectangle.left;
	auto ySize = rectangle.bottom - rectangle.top;
	auto fractionx = xSize / oldSize.width;
	auto fractiony = ySize / oldSize.height;
	scaleToSize(fractionx * newsize.width, fractiony * newsize.height);
}