#pragma once

#include <vector>

#include "VisualLayer.h"

class GraphicRegistry;
class GraphicContainer;

class ArenaDisplay
{
public:
	ArenaDisplay(GraphicRegistry* registry);

	void addLayer() { layers.push_back({ registry }); }

	VisualLayer& getLayer(int index) { return layers[index]; }
	const VisualLayer& getLayer(int index) const { return layers[index]; }

	void draw(ID2D1HwndRenderTarget* renderTarget);

private:
	GraphicRegistry* registry;
	GraphicContainer* background;
	std::vector<VisualLayer> layers;

};

