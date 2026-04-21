#include "stdafx.h"
#include "GraphicRegistry.h"

void GraphicRegistry::addGraphic(GraphicContainer* graphic, int index)
{
	if (graphics.size() > index)
	{
		graphics[index] = graphic;
	}
	else
	{
		while (graphics.size() < index)
		{
			graphics.push_back(nullptr);
		}
		graphics.push_back(graphic);
	}
}

GraphicContainer* GraphicRegistry::getGraphic(int index) const
{
	return graphics[index];
}

void GraphicRegistry::rescaleAll(D2D1_SIZE_U oldSize, ID2D1HwndRenderTarget* newRenderTarget)
{
	for (auto& graphic : graphics)
	{
		graphic->rescale(oldSize, newRenderTarget);
	}
}