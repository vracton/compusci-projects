#pragma once
#include <DirectXMath.h>
#include "GeometryGenerator.h"

class Visualizer3DCore;

class VisualizerCommand
{
public:
	virtual void execute(Visualizer3DCore& core) const = 0;
};

class AddMeshDataCommand : public VisualizerCommand
{
public:
	AddMeshDataCommand(int index, const GeometryGenerator::MeshData& mesh);
	virtual void execute(Visualizer3DCore& core) const override;

private:
	const int index;
	const GeometryGenerator::MeshData mesh;
};

class AddMaterialCommand : public VisualizerCommand
{
public:
	AddMaterialCommand(int index, DirectX::XMVECTORF32 color,
		float fresnel, float roughness);
	virtual void execute(Visualizer3DCore& core) const override;

private:
	const int index;
	const DirectX::XMVECTORF32 color;
	const float fresnel;
	const float roughness;
};

class ChangeMaterialColorCommand : public VisualizerCommand
{
public:
	ChangeMaterialColorCommand(int index, DirectX::XMVECTORF32 color);
	ChangeMaterialColorCommand(int index, float red, float green, float blue, float alpha);
	virtual void execute(Visualizer3DCore& core) const override;

private:
	const int index;
	const DirectX::XMVECTORF32 color;
};

class AddObjectCommand : public VisualizerCommand
{
public:
	AddObjectCommand(int object, DirectX::XMFLOAT3 scale, DirectX::XMFLOAT3X3 rotation,
		DirectX::XMFLOAT3 position, int materialIndex, int shapeIndex);
	virtual void execute(Visualizer3DCore& core) const override;

private:
	int object;
	const DirectX::XMFLOAT3 scale;
	const DirectX::XMFLOAT3X3 rotation;
	const DirectX::XMFLOAT3 position;
	const int materialIndex; 
	const int shapeIndex;
};

class MoveObjectCommand : public VisualizerCommand
{
public:
	MoveObjectCommand(int index, DirectX::XMFLOAT3 newPosition);
	virtual void execute(Visualizer3DCore& core) const override;

private:
	const int index;
	const DirectX::XMFLOAT3 position;
};

class TransformObjectCommand : public VisualizerCommand
{
public:
	TransformObjectCommand(int index, DirectX::XMFLOAT3 newScale, DirectX::XMFLOAT3X3 newRotation,
		DirectX::XMFLOAT3 newPosition);
	virtual void execute(Visualizer3DCore& core) const override;

private:
	const int index;
	const DirectX::XMFLOAT3 scale;
	const DirectX::XMFLOAT3X3 rotation;
	const DirectX::XMFLOAT3 position;
};

class RemoveObjectCommand : public VisualizerCommand
{
public:
	RemoveObjectCommand(int index);
	virtual void execute(Visualizer3DCore& core) const override;

private:
	const int index;
};

class RemoveAllObjectsCommand : public VisualizerCommand
{
public:
	virtual void execute(Visualizer3DCore& core) const override;
};

class ClearAllCommand : public VisualizerCommand
{
public:
	virtual void execute(Visualizer3DCore& core) const override;
};
