//***************************************************************************************
// Camera.h by Frank Luna (C) 2011 All Rights Reserved.
// Modified by Peter Dong
//   
// Simple first person style camera class that lets the viewer explore the 3D scene.
//   -It keeps track of the camera coordinate system relative to the world space
//    so that the view matrix can be constructed.  
//   -It keeps track of the viewing frustum of the camera so that the projection
//    matrix can be obtained.
//***************************************************************************************

#ifndef CAMERA_H
#define CAMERA_H

#include "d3dUtil.h"

class Camera
{
public:

	Camera();
	~Camera();

	// Get/Set world camera position.
	DirectX::XMVECTOR getPosition() const; // As a 4-component DirectX hardware-accelerated vector
	DirectX::XMFLOAT3 getPosition3f() const; // as a simple (but not hardware-accelerated) three-component vector
	void setPosition(float x, float y, float z);
	void setPosition(const DirectX::XMFLOAT3& v);

	// Get camera basis vectors.
	DirectX::XMVECTOR getRight() const;
	DirectX::XMFLOAT3 getRight3f() const;
	DirectX::XMVECTOR getUp() const;
	DirectX::XMFLOAT3 getUp3f() const;
	DirectX::XMVECTOR getLook() const;
	DirectX::XMFLOAT3 getLook3f() const;

	// Get frustum properties.
	float getNearZ() const;
	float getFarZ() const;
	float getAspect() const;
	float getFovY() const;
	float getFovX() const;

	// Get near and far plane dimensions in view space coordinates.
	float getNearWindowWidth() const;
	float getNearWindowHeight() const;
	float getFarWindowWidth() const;
	float getFarWindowHeight() const;

	// Set frustum.
	void setLens(float fovY, float aspect, float zn, float zf);

	// Scale all relevant camera stuff
	void scaleAll(float scaleFactor);

	// Define camera space via lookAt parameters.
	void lookAt(DirectX::FXMVECTOR pos, DirectX::FXMVECTOR target, DirectX::FXMVECTOR worldUp); // FXMVECTOR is an XMVECTOR optimized for efficient function calls
	void lookAt(const DirectX::XMFLOAT3& pos, const DirectX::XMFLOAT3& target, const DirectX::XMFLOAT3& up);

	// Get view/projection matrices.
	// Stored as memory-friendly XMFLOAT4X4 matrices but converted to XMMATRIX for calculations when you use these accessors
	DirectX::XMMATRIX getView() const;
	DirectX::XMMATRIX getProj() const;

	DirectX::XMFLOAT4X4 getView4x4f() const;
	DirectX::XMFLOAT4X4 getProj4x4f() const;

	// strafe/walk the camera a distance d.
	void strafe(float d);
	void walk(float d);
	void elevate(float d);

	// Rotate the camera.
	void pitch(float angle);
	void roll(float angle);
	void yaw(float angle);
	void rotateY(float angle); // Rotate about the world y axis in world space

	// After modifying camera position/orientation, call this to rebuild the view matrix.
	void updateViewMatrix() const;

private:

	// Camera coordinate system with coordinates relative to world space.
	mutable DirectX::XMFLOAT3 mPosition = { 0.0f, 0.0f, 0.0f };
	mutable DirectX::XMFLOAT3 mRight = { 1.0f, 0.0f, 0.0f };
	mutable DirectX::XMFLOAT3 mUp = { 0.0f, 1.0f, 0.0f };
	mutable DirectX::XMFLOAT3 mLook = { 0.0f, 0.0f, 1.0f };

	// Cache frustum properties.
	float mNearZ = 0.0f;
	float mFarZ = 0.0f;
	float mAspect = 0.0f;
	float mFovY = 0.0f;
	float mNearWindowHeight = 0.0f;
	float mFarWindowHeight = 0.0f;

	// Keeps track of when the view needs to be recalculated
	mutable bool mViewDirty = true;

	// Cache View/Proj matrices.
	mutable DirectX::XMFLOAT4X4 mView = MathHelper::Identity4x4();
	mutable DirectX::XMFLOAT4X4 mProj = MathHelper::Identity4x4();
};

#endif // CAMERA_H