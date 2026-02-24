#pragma once

#include <vector>
#include <DirectXMath.h>

// Represents a view frustrum defined by a set of 3D points
class Frustrum
{
public:
	// Gets the field of view in the Y direction (in radians)
	float getFOVy() const;
	// Gets the aspect ratio (width / height)
	float getAspectRatio() const;
	// Gets the near plane distance
	float getNearPlane() const;
	// Gets the far plane distance
	float getFarPlane() const;

	// Adds a point to the frustrum definition
	void addPoint(DirectX::XMFLOAT3 point);

private:
	bool initialized = false;

	DirectX::XMFLOAT3 bottomLowerCorner;
	float topZ;
	float aspectRatio;
};

