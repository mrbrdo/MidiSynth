#include "StdAfx.h"
#include "WaveOutDevice.h"

std::vector<WaveOutDevice *> devices;

WaveOutDevice *getDeviceById(DWORD id)
{
	for (int i=0; i<(int)devices.size(); i++)
	{
		if (devices[i]->Identifier() == id)
		{
			return devices[i];
		}
	}
	return NULL;
}

void CALLBACK waveOutProc(
    HWAVEOUT hwo,
    UINT uMsg,
    WaveOutDevice *device,
    LPWAVEHDR header,
    DWORD_PTR dwParam2
)
{
	// if (device->Destroying()) return; // not necessary anymore since we don't call any waveOut* APIs here
	if (uMsg == WOM_DONE)
	{
		device->BufferDone(header);
	}
}

WaveOutDevice::WaveOutDevice(int sampleRate, int nChannels, int bitsPerSample, int bufferSize, LPCSTR lpSyncName)
{
	destroying = false;
    WAVEFORMATEX  format;

    // Open a waveform device for output using window callback. 

	format.wFormatTag = WAVE_FORMAT_PCM;
	format.nChannels = nChannels;
	format.wBitsPerSample = bitsPerSample;
	// common values for nSamplesPerSec are 8.0 kHz, 11.025 kHz, 22.05 kHz, and 44.1 kHz (in Hertz)
	format.nSamplesPerSec = sampleRate;
	
	// PCM default
	format.cbSize = 0;
	format.nBlockAlign = format.nChannels * (format.wBitsPerSample / 8);
	format.nAvgBytesPerSec = format.nSamplesPerSec * format.nBlockAlign;

    if (waveOutOpen((LPHWAVEOUT)&device, WAVE_MAPPER, 
                    &format, 
                    (DWORD_PTR) waveOutProc, (DWORD_PTR) this, CALLBACK_FUNCTION)) 
    { 
        MessageBox(NULL, 
                   L"Failed to open waveform output device.", 
                   NULL, MB_OK | MB_ICONEXCLAMATION);
		device = 0;
    }
	
	this->nBuffers = 2;

	// sync object
	semaphore = CreateSemaphoreA(NULL, 0, nBuffers, lpSyncName);

	// buffers
	this->bufferSize = bufferSize;

	lpBufferData = new HPSTR[nBuffers];
	lpBufferHeader = new LPWAVEHDR[nBuffers];
	
	iWriteBuffer = 0;

	for (int i=0; i<nBuffers; i++)
	{
		lpBufferData[i] = new char[bufferSize];

		lpBufferHeader[i] = new WAVEHDR();

		lpBufferHeader[i]->lpData = lpBufferData[i];
		lpBufferHeader[i]->dwBufferLength = bufferSize;
		lpBufferHeader[i]->dwFlags = 0L;
		lpBufferHeader[i]->dwLoops = 0L;
		waveOutPrepareHeader(device, lpBufferHeader[i], sizeof(WAVEHDR));

		// init buffer to 127 (silence)
		for (int j=0; j<bufferSize; j++)
		{
			lpBufferData[i][j] = 127;
		}
		// Enqueue buffer for the first time
		EnqueueWriteBuffer();
	}
}

DWORD WaveOutDevice::Identifier()
{
	return (DWORD) device;
}

bool WaveOutDevice::Open()
{
	return (device != 0);
}

void WaveOutDevice::Write(LPCSTR data, int szData)
{
	if (szData > bufferSize) return;

	CopyMemory(lpBufferData[iWriteBuffer], data, szData);
}

int WaveOutDevice::getNextBuffer(int i)
{
	return (i + 1) % nBuffers;
}

void WaveOutDevice::EnqueueWriteBuffer()
{
	//@TODO: handle errors
	//waveOutPrepareHeader(device, lpBufferHeader[iWriteBuffer], sizeof(WAVEHDR));
	// only need to prepare once

	waveOutWrite(device, lpBufferHeader[iWriteBuffer], sizeof(WAVEHDR));
	iWriteBuffer = getNextBuffer(iWriteBuffer);
}

void WaveOutDevice::BufferDone(LPWAVEHDR header)
{
	//waveOutUnprepareHeader(device, header, sizeof(WAVEHDR));
	// waveOutUnprepareHeader shouldn't be required, and if called
	// during waveOutReset (in destructor) it will cause a deadlock
	header->dwFlags = WHDR_PREPARED;//0L;

	// signal application that a buffer is free to write to
	ReleaseSemaphore(semaphore, 1, NULL);
}

bool WaveOutDevice::Destroying()
{
	return destroying;
}

WaveOutDevice::~WaveOutDevice(void)
{
	destroying = true;
	// waveOutReset calls the callback if it marks any
	// buffers as done, but calling any waveOut* APIs
	// while resetting will cause a deadlock
    waveOutReset(device); // cancel any currently playing buffers
    waveOutClose(device);

	for (int i=0; i<nBuffers; i++) {
		delete lpBufferHeader[i];
		delete lpBufferData[i];
	}
}
