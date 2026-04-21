#pragma once

#include <vector>
#include <iostream>

#include "Coordinate.h"

class GraphicContainer;
class GraphicRegistry;
struct ID2D1HwndRenderTarget;

class VisualLayer
{
public:
	VisualLayer(GraphicRegistry* iregistry) : registry(iregistry) {}

	void addGraphic(int index, int graphicIndex, Coordinate<double> location)
	{
		spriteMap.insert({ index, {graphicIndex, location} });
	}

	void moveGraphic(int index, Coordinate<double> newLocation)
	{
		//std::cout << "Current position: " << spriteMap[index].position.getX() << ", "
		//	<< spriteMap[index].position.getY() << "\nNew position: " << newLocation.getX()
		//	<< ", " << newLocation.getY();
		spriteMap[index].position = newLocation;
	}

	void rotateGraphic(int index, double newRotation)
	{
		spriteMap[index].rotation = newRotation;
	}

	void removeGraphic(int index)
	{
		spriteMap.erase(index);
	}

	void changeGraphic(int index, int newGraphicIndex)
	{
		spriteMap[index].graphicIndex = newGraphicIndex;
	}

	void draw(ID2D1HwndRenderTarget* renderTarget);

private:
	GraphicRegistry* registry;

	struct IndexPosition
	{
		int graphicIndex;
		Coordinate<double> position;
		double rotation; // in radians

		IndexPosition(int index, double x, double y, double irotation = 0) :
			graphicIndex(index),
			position(x, y),
			rotation(irotation)
		{}

		IndexPosition(int index, Coordinate<double> coord, double irotation = 0) :
			graphicIndex(index),
			position(coord),
			rotation(irotation)
		{}

		IndexPosition() :
			graphicIndex(0),
			position(0, 0),
			rotation(0)
		{}
	};

	std::unordered_map<int, IndexPosition> spriteMap;
};

