#pragma once
#include <Windows.h>

class WaveOutDevice
{
private:
	HWAVEOUT device;

	int nBuffers;
	int bufferSize;
	int iWriteBuffer;
	bool destroying;
	HPSTR *lpBufferData;
	LPWAVEHDR *lpBufferHeader;

	HANDLE semaphore;
	
	int getNextBuffer(int i);
public:
	WaveOutDevice(int sampleRate, int nChannels, int bitsPerSample, int bufferSize, LPCSTR lpSyncName);
	void BufferDone(LPWAVEHDR header);
	void EnqueueWriteBuffer();
	void Write(LPCSTR data, int szData);
	bool Destroying();
	DWORD Identifier();
	bool Open();
	~WaveOutDevice(void);
};

extern std::vector<WaveOutDevice *> devices;
WaveOutDevice *getDeviceById(DWORD id);