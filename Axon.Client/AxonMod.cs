﻿using Axon.Client;
using Axon.Client.AssetBundle;
using Axon.Client.Command;
using Axon.Client.Event;
using Axon.Client.Meta;
using Axon.NetworkMessages;
using Il2Cpp;
using Il2CppCommandSystem;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Runtime;
using Il2CppMirror;
using MelonLoader;
using CommandHandler = Axon.Client.Command.CommandHandler;

[assembly: MelonInfo(typeof(AxonMod), "Axon", "0.0.1", "Dimenzio & Tiliboyy")]
[assembly: MelonGame("Northwood", "SCPSL")]
namespace Axon.Client;

public class AxonMod : MelonMod
{
    public static readonly Version AxonVersion = new Version(0, 0, 1);

    public static AxonMod Instance { get; private set; }
    public static EventManager EventManager { get; private set; }

    public override void OnInitializeMelon()
    {
        CustomNetworkManager.Modded = true;
        Il2CppCentralAuth.CentralAuthManager.NoAuthStartupArg = "yes?";

        Instance = this;
        EventManager = new EventManager();

        MetaAnalyzer.Init();
        EventManager.Init();
        AssetBundleManager.Init();
        CommandHandler.Init();

        //Analyze should always be called last so that all handlers/events are registered
        MetaAnalyzer.Analyze();
        RegisterTypes();
        LoggerInstance.Msg("Axon Loaded");
    }

    private void RegisterTypes()
    {
        ClassInjector.RegisterTypeInIl2Cpp<TestMessage>(new RegisterTypeOptions()
        {
            Interfaces = new Type[] { typeof(NetworkMessage) }
        });

        Writer<TestMessage>.write = new Action<NetworkWriter, TestMessage>(WriteTest);
        Reader<TestMessage>.read = new Func<NetworkReader, TestMessage>(ReadTest);
    }

    private void WriteTest(NetworkWriter writer, TestMessage message)
    {
        writer.WriteString(message.message);
    }

    private TestMessage ReadTest(NetworkReader reader)
    {
        return null;
    }
}