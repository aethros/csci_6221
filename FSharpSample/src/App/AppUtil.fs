namespace App

open System
open System.Security.Principal
open System.Runtime.InteropServices

module AppUtil =
    [<DllImport("libc", EntryPoint = "geteuid")>]
    extern uint32 geteuid()

    let isAdministrator () =
        try
            let identity = WindowsIdentity.GetCurrent()
            let principal = WindowsPrincipal identity
            principal.IsInRole WindowsBuiltInRole.Administrator
        with
        | ex ->
            printfn "An error occurred while checking admin rights: %s" ex.Message
            false

    let isElevated () =
        geteuid() = 0u

    let isWindows () =
        Environment.OSVersion.Platform.ToString().StartsWith "Win"