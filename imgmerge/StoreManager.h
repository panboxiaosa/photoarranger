#pragma once
#include "stdafx.h"
#include "Fragment.h"


class StoreManager
{
public:
	StoreManager();
	~StoreManager();

	void saveTo(string tar);

	void saveDefault();

	void loadDefault();

	void load(string tar);

	void build(string tar);

private:
	vector<Fragment> fragments;
	int pixelWidth;
	int pixelHeight;
	int dpi;
	
};

