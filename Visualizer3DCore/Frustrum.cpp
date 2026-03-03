#include "Frustrum.h"

#include <algorithm>
#include <cmath>

#include "MathHelper.h"

float Frustrum::getFOVy() const
{
	float angle = std::atan(bottomLowerCorner.y / bottomLowerCorner.z);
	return angle * 2;
}

float Frustrum::getAspectRatio() const
{
	return bottomLowerCorner.x / bottomLowerCorner.y;
}

float Frustrum::getNearPlane() const
{
	return topZ;
}

float Frustrum::getFarPlane() const
{
	return bottomLowerCorner.z;
}

void Frustrum::addPoint(DirectX::XMFLOAT3 point)
{
	// Note: z must be positive

	if (!initialized)
	{
		topZ = point.z / 2;
		bottomLowerCorner.z = point.z * 10;
		float angle = MathHelper::Pi / 8;
		float deltaXorY = bottomLowerCorner.z * std::tan(angle);

		float deltaXorYAtTop = deltaXorY / bottomLowerCorner.z * topZ;

		if (std::abs(point.x) > deltaXorYAtTop)
		{
			bottomLowerCorner.x = std::abs(point.x) * bottomLowerCorner.z / point.z;
		}
		else
		{
			bottomLowerCorner.x = deltaXorY;
		}

		if (std::abs(point.y) > deltaXorYAtTop)
		{
			bottomLowerCorner.y = std::abs(point.y) * bottomLowerCorner.z / point.z;
		}
		else
		{
			bottomLowerCorner.x = deltaXorY;
		}

		initialized = true;
	}
	else
	{
		if (point.z < topZ)
		{
			topZ = point.z;
		}
		if (point.z > bottomLowerCorner.z)
		{
			bottomLowerCorner.z = point.z;
		}

		float ratioX = std::abs(point.x) / point.z;
		float currentRatioX = bottomLowerCorner.x / bottomLowerCorner.z;

		if (ratioX > currentRatioX)
		{
			bottomLowerCorner.x = point.x / point.z * bottomLowerCorner.z;
		}

		float ratioY = std::abs(point.y) / point.z;
		float currentRatioY = bottomLowerCorner.y / bottomLowerCorner.z;

		if (ratioY > currentRatioY)
		{
			bottomLowerCorner.y = point.y / point.z * bottomLowerCorner.z;
		}
	}
}
