﻿using Axon.Shared.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axon.Client.API.Features;
using System.Security.Cryptography;
using Il2Cpp;
using MelonLoader;
using System.Xml.Serialization;
using System.Reflection.Metadata;
using Il2CppLiteNetLib.Utils;
using Il2CppLiteNetLib;

namespace Axon.Client;

public static class AuthHandler
{
    private static Dictionary<string, byte[]> _decrypted = new Dictionary<string, byte[]>();

    public static string AuthFilePath { get; private set; }
    public static PlayerAuth PlayerAuth { get; private set; }
    public static RsaService RsaService { get; private set; }

    internal static void Init()
    {
        AuthFilePath = Path.Combine(Paths.AxonPath, "user.xml");
        if (!File.Exists(AuthFilePath))
            CreateNew();

        var serializer = new XmlSerializer(typeof(PlayerAuth));
        var stream = new FileStream(AuthFilePath, FileMode.Open, FileAccess.Read);
        PlayerAuth = (PlayerAuth)serializer.Deserialize(stream);
        stream.Close();
        RsaService = new RsaService(PlayerAuth.Key);

        MelonLogger.Msg(AuthUtility.GetUserIdFromPub(PlayerAuth.Key));
    }

    internal static void AuthWrite(NetDataWriter writer)
    {
        var server = CustomLiteNetLib4MirrorTransport.Singleton.clientAddress;
        var decrypted = new byte[0];
        var isSecondTime = false;

        if (_decrypted.ContainsKey(server))
        {
            decrypted = _decrypted[server];
            isSecondTime = true;
        }

        writer.Put((byte)(isSecondTime ? 4 : 3));
        writer.Put(Il2CppGameCore.Version.Major);
        writer.Put(Il2CppGameCore.Version.Minor);
        writer.Put(Il2CppGameCore.Version.Revision);
        writer.Put(Il2CppGameCore.Version.BackwardCompatibility);
        if (Il2CppGameCore.Version.BackwardCompatibility)
            writer.Put(Il2CppGameCore.Version.BackwardRevision);


        writer.PutBytesWithLength(PlayerAuth.Key.Exponent);
        writer.PutBytesWithLength(PlayerAuth.Key.Modulus);
        writer.Put(PlayerAuth.UserId);
        writer.Put(PlayerAuth.Username);

        if (isSecondTime)
        {
            writer.PutBytesWithLength(decrypted);
            _decrypted.Remove(server);
        }
    }

    internal static void RejectAuth(DisconnectInfo info)
    {
        MelonLogger.Warning("Rejected Auth");
        var data = info.AdditionalData;
        var requestType = data.GetByte();
        if (requestType != 100)
        {
            info.AdditionalData._position = 0;
            return;
        }

        var encrypted = data.GetBytesWithLength();
        var decrypted = RsaService.Decript(encrypted);
        var server = CustomLiteNetLib4MirrorTransport.Singleton.clientAddress;
        _decrypted[server] = decrypted;
    }

    private static void CreateNew()
    {
        var csp = new RSACryptoServiceProvider(2048);
        var auth = new PlayerAuth();
        auth.Key = csp.ExportParameters(true);
        auth.Username = Welcome.CurrentNickname;
        auth.UserId = AuthUtility.GetUserIdFromPub(auth.Key);

        var sw = new StringWriter();
        var serializer = new XmlSerializer(typeof(PlayerAuth));
        serializer.Serialize(sw, auth);

        File.Create(AuthFilePath).Close();
        File.WriteAllText(AuthFilePath, sw.ToString(), Encoding.Unicode);
    }
}
