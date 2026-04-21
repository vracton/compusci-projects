#pragma once

#include <vector>

#include "GraphicContainer.h"

class GraphicRegistry
{
public:
	void addGraphic(GraphicContainer* graphic, int index);
	GraphicContainer* getGraphic(int index) const;
	std::vector<GraphicContainer*>& getAllGraphics() { return graphics; }

	void rescaleAll(D2D1_SIZE_U oldSize, ID2D1HwndRenderTarget* newRenderTarget);

private:
	std::vector<GraphicContainer*> graphics;
};

