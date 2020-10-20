// TestCppConsoleApp.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <Windows.h>
#include <iostream>
#include <string>
#include <winnt.h>
#include <WinBase.h>

int main()
{
    long long lint = 2251799814519839;

    HANDLE hVolumeHint = CreateFileW(
        L".",
        GENERIC_READ,
        FILE_SHARE_READ |
        FILE_SHARE_WRITE |
        FILE_SHARE_DELETE,
        NULL,
        OPEN_EXISTING,
        FILE_FLAG_BACKUP_SEMANTICS,
        NULL);

    if (hVolumeHint == INVALID_HANDLE_VALUE)
    {
        std::cout << "Error: " << GetLastError();
        return -2;
    }

    FILE_ID_DESCRIPTOR fileId = FILE_ID_DESCRIPTOR();
    fileId.dwSize = sizeof(FILE_ID_DESCRIPTOR);
    fileId.FileId.QuadPart = lint;
    fileId.Type = FILE_ID_TYPE::FileIdType;

    HANDLE hwnd = OpenFileById(
        hVolumeHint,
        &fileId,
        GENERIC_READ,
        FILE_SHARE_READ | FILE_SHARE_WRITE,
        NULL,
        FILE_FLAG_BACKUP_SEMANTICS
    );
    CloseHandle(hVolumeHint);

    if (hwnd == INVALID_HANDLE_VALUE)
    {
        DWORD lastErrorCode = GetLastError();
        wchar_t err[256];
        memset(err, 0, 256);
        FormatMessage(
            FORMAT_MESSAGE_FROM_SYSTEM,
            NULL,
            lastErrorCode,
            MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
            err,
            255,
            NULL
        );

        std::cout << "Error: " << err << " (" << lastErrorCode << ")";
    }
    else
    {
        wchar_t pathName[MAX_PATH];

        GetFinalPathNameByHandleW(
            hwnd,
            pathName,
            MAX_PATH,
            0
        );

        printf("Path: %S\n", pathName);
    }

    CloseHandle(hwnd);
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
