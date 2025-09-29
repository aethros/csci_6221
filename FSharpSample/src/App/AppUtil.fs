namespace App

open System
open System.Security.Principal
open System.Runtime.InteropServices

module AppUtil =
    [<DllImport("libc", EntryPoint = "geteuid")>]
    extern uint32 geteuid()

    let isAdministrator () =
        try
            (WindowsPrincipal
                (WindowsIdentity.GetCurrent()))
                    .IsInRole WindowsBuiltInRole.Administrator
        with e ->
            printfn "An error occurred while checking admin rights: %s" e.Message
            false

    let isElevated () =
        geteuid () = 0u

    let isWindows () =
        Environment.OSVersion
            .Platform.ToString().StartsWith "Win"
