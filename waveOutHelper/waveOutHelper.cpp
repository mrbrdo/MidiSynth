// waveOutHelper.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "WaveOutDevice.h"

// Functions are dll exported through the .def file

DWORD __stdcall OpenWaveDevice(int sampleRate, int bufferSize, LPCSTR lpSyncName)
{
	WaveOutDevice *dev = new WaveOutDevice(sampleRate, bufferSize, lpSyncName);
	devices.push_back(dev);
	return dev->Identifier();
}

void __stdcall CloseWaveDevice(DWORD deviceId)
{
	for (int i=0; i<devices.size(); i++)
	{
		if (devices[i]->Identifier() == deviceId)
		{
			WaveOutDevice *dev = devices[i];
			devices.erase(devices.begin() + i);
			delete dev;
			i--;
		}
	}
}

void __stdcall WriteWaveBuffer(DWORD deviceId, LPCSTR input, DWORD szInput)
{
	WaveOutDevice *dev = getDeviceById(deviceId);
	if (dev != NULL)
	{
		dev->Write(input, szInput);
		dev->EnqueueWriteBuffer();
	}
}