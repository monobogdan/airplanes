#include "dxsharp.h"

namespace DXSharp
{
	namespace Sound
	{
		DirectSound::DirectSound()
		{
			IDirectSound* dSound;

			Guard(DirectSoundCreate(0, &dSound, 0));
			this->dsound = dSound;
		}

		DirectSound::~DirectSound()
		{
			dsound->Release();
		}

		void DirectSound::Initialize()
		{
			Guard(dsound->SetCooperativeLevel(GetForegroundWindow(), DSSCL_NORMAL));
		}

		void DirectSound::Compact()
		{
			Guard(dsound->Compact());
		}

		SoundBuffer^ DirectSound::CreateSoundBuffer(BufferDescription desc)
		{
			WAVEFORMATEX waveFormat;
			waveFormat.cbSize = 0;
			waveFormat.nAvgBytesPerSec = desc.Format.AvgBytesPerSec;
			waveFormat.nBlockAlign = desc.Format.BlockAlign;
			waveFormat.nChannels = desc.Format.Channels;
			waveFormat.nSamplesPerSec = desc.Format.SamplesPerSec;
			waveFormat.wBitsPerSample = desc.Format.BitsPerSample;
			waveFormat.wFormatTag = desc.Format.FormatTag;

			DSBUFFERDESC bufDesc;
			memset(&bufDesc, 0, sizeof(bufDesc));
			bufDesc.dwBufferBytes = desc.BufferBytes;
			bufDesc.dwFlags = (DWORD)desc.Flags;
			bufDesc.dwReserved = 0;
			bufDesc.dwSize = sizeof(bufDesc);

			if(!(bufDesc.dwFlags & DSBCAPS_PRIMARYBUFFER))
				bufDesc.lpwfxFormat = &waveFormat;

			IDirectSoundBuffer* buf;

			Guard(dsound->CreateSoundBuffer(&bufDesc, &buf, 0));

			return gcnew SoundBuffer(buf, desc);
		}

		SoundBuffer::SoundBuffer(IDirectSoundBuffer* buffer, BufferDescription desc)
		{
			this->buffer = buffer;
			this->description = desc;
		}

		IntPtr SoundBuffer::Lock()
		{
			void* ptr = 0;
			DWORD buf1Locked = 0;
			Guard(buffer->Lock(0, description.BufferBytes, &ptr, &buf1Locked, 0, 0, DSBLOCK_ENTIREBUFFER));

			lockPtr = ptr;
			lockBytes = buf1Locked;

			return IntPtr(ptr);
		}

		void SoundBuffer::Play()
		{
			buffer->Play(0, 0, DSBPLAY_LOOPING);
		}

		void SoundBuffer::Stop()
		{
			buffer->Stop();
		}

		void SoundBuffer::Unlock()
		{
			Guard(buffer->Unlock(lockPtr, lockBytes, 0, 0));
		}

	}
}