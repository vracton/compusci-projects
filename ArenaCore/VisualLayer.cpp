#include "stdafx.h"
#include "VisualLayer.h"

#include "GraphicContainer.h"
#include "GraphicRegistry.h"

void VisualLayer::draw(ID2D1HwndRenderTarget* renderTarget)
{
	auto drawWindow = renderTarget->GetPixelSize();

	for (const auto& entry : spriteMap)
	{
		auto indexPosition = entry.second;
		auto graphic = registry->getGraphic(indexPosition.graphicIndex);
		graphic->draw(indexPosition.position.getX() * drawWindow.width,
			indexPosition.position.getY() * drawWindow.height,
			renderTarget, indexPosition.rotation);
	}
}