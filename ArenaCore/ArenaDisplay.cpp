#include "stdafx.h"
#include "ArenaDisplay.h"

#include "GraphicContainer.h"
#include "GraphicRegistry.h"

ArenaDisplay::ArenaDisplay(GraphicRegistry* iregistry) :
	registry(iregistry),
	background(nullptr)
{
}

void ArenaDisplay::draw(ID2D1HwndRenderTarget* renderTarget)
{
	for (auto& layer : layers)
	{
		layer.draw(renderTarget);
	}
}