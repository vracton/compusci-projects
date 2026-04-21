#pragma once

#include <unordered_map>

template<typename T>
class Coordinate
{
public:
	Coordinate(T ix, T iy) : x(ix), y(iy) {}

	T getX() const { return x; }
	T getY() const { return y; }
	void setX(T value) { x = value; }
	void setY(T value) { y = value; }

private:
	T x;
	T y;
};

template<typename T>
bool operator==(const Coordinate<T>& lhs, const Coordinate<T>& rhs)
{
	return lhs.getX() == rhs.getX() && lhs.getY() == rhs.getY();
}

template<typename T>
bool operator!=(const Coordinate<T>& lhs, const Coordinate<T>& rhs)
{
	return !(lhs == rhs);
}

template<typename T>
bool sortFunction(const Coordinate<T>& lhs, const Coordinate<T>& rhs)
{
	if (lhs.getX() < rhs.getX())
		return true;
	else if (rhs.getX() < lhs.getX())
		return false;
	else
		return lhs.getY() < rhs.getY();
}

// Many thanks to https://prateekvjoshi.com/2014/06/05/using-hash-function-in-c-for-user-defined-classes/
namespace std
{
	template <typename T>
	struct hash<Coordinate<T>>
	{
		size_t operator()(const Coordinate<T>& coord) const
		{
			// Compute individual hash values for two data members and combine them using XOR and bit shifting
			return ((hash<T>()(coord.getX()) ^ (hash<T>()(coord.getY) << 1)) >> 1);
		}
	};
}