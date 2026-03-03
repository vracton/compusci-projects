#include "VisualizerCommand.h"

#include "GeometryGenerator.h"
#include "Visualizer3DCore.h"


MoveObjectCommand::MoveObjectCommand(int i, DirectX::XMFLOAT3 newPosition) :
	index(i),
	position(newPosition)
{}

void MoveObjectCommand::execute(Visualizer3DCore& core) const
{
	core.moveObject(index, position);
}

AddObjectCommand::AddObjectCommand(int iobject, DirectX::XMFLOAT3 iscale, DirectX::XMFLOAT3X3 irotation, DirectX::XMFLOAT3 iposition, int imaterialIndex, int ishapeIndex) :
	object(iobject),
	scale(iscale),
	position(iposition),
	rotation(irotation),
	materialIndex(imaterialIndex),
	shapeIndex(ishapeIndex)
{}

void AddObjectCommand::execute(Visualizer3DCore& core) const
{
	core.addObject(object, scale, rotation, position, materialIndex, shapeIndex);
}

TransformObjectCommand::TransformObjectCommand(int i, DirectX::XMFLOAT3 newScale, DirectX::XMFLOAT3X3 newRotation, DirectX::XMFLOAT3 newPosition) :
	index(i),
	scale(newScale),
	rotation(newRotation),
	position(newPosition)
{
}

void TransformObjectCommand::execute(Visualizer3DCore& core) const
{
	core.transformObject(index, scale, rotation, position);
}

AddMeshDataCommand::AddMeshDataCommand(int i, const GeometryGenerator::MeshData& imesh) :
	index(i),
	mesh(imesh)
{}

void AddMeshDataCommand::execute(Visualizer3DCore& core) const
{
	core.addMeshData(index, mesh);
}

AddMaterialCommand::AddMaterialCommand(int i, DirectX::XMVECTORF32 icolor, float ifresnel, float iroughness) :
	index(i),
	color(icolor),
	fresnel(ifresnel),
	roughness(iroughness)
{
}

void AddMaterialCommand::execute(Visualizer3DCore& core) const
{
	core.addMaterial(index, color, fresnel, roughness);
}

RemoveObjectCommand::RemoveObjectCommand(int i) :
	index(i)
{}

void RemoveObjectCommand::execute(Visualizer3DCore& core) const
{
	core.removeObject(index);
}

void RemoveAllObjectsCommand::execute(Visualizer3DCore& core) const
{
	core.removeAllObjects();
}

void ClearAllCommand::execute(Visualizer3DCore& core) const
{
	core.clearAll();
}

ChangeMaterialColorCommand::ChangeMaterialColorCommand(int i, DirectX::XMVECTORF32 icolor) :
	index(i),
	color(icolor)
{
}

ChangeMaterialColorCommand::ChangeMaterialColorCommand(int i, float red, float green, float blue, float alpha) :
	index(i),
	color{ red, green, blue, alpha }
{
}

void ChangeMaterialColorCommand::execute(Visualizer3DCore& core) const
{
	core.changeMaterialColor(index, color);
}

