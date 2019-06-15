﻿#region Using
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
#endregion
namespace FakeManager
{
    [ApiVersion(2, 1)]
    public class FakeManager : TerrariaPlugin
    {
        #region Data

        public override string Name => "FakeManager";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public override string Author => "Anzhelika & ASgo";
        public override string Description => "Plugin for creating zones with fake tiles and signs.";
        public FakeManager(Main game) : base(game) { }

        public static FakeCollection Common = new FakeCollection();
        //public static FakeCollection[] Personal = new FakeCollection[Main.maxPlayers];
        internal static int[] AllPlayers;

        #endregion

        #region Initialize

        public override void Initialize()
        {
            AllPlayers = new int[Main.maxPlayers];
            for (int i = 0; i < Main.maxPlayers; i++)
                AllPlayers[i] = i;

            ServerApi.Hooks.NetSendData.Register(this, OnSendData, int.MaxValue);
            /*
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            */
        }

        #endregion
        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetSendData.Deregister(this, OnSendData);
                /*
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnServerLeave);
                */
            }
            base.Dispose(disposing);
        }

        #endregion

        #region OnSendData

        private void OnSendData(SendDataEventArgs args)
        {
            if (args.Handled)
                return;

            switch (args.MsgId)
            {
                case PacketTypes.TileSendSection:
                    args.Handled = true;
                    SendSectionPacket.Send(args.remoteClient, args.ignoreClient,
                        args.number, (int)args.number2, (short)args.number3, (short)args.number4);
                    break;
                case PacketTypes.TileSendSquare:
                    args.Handled = true;
                    SendTileSquarePacket.Send(args.remoteClient, args.ignoreClient,
                        args.number, (int)args.number2, (int)args.number3, args.number5);
                    break;
            }
        }

        #endregion
        #region OnServerJoin, OnServerLeave
        /*
        private void OnServerJoin(JoinEventArgs args) =>
            Personal[args.Who] = new FakeCollection(true);

        private void OnServerLeave(LeaveEventArgs args)
        {
            FakeCollection collection = Personal[args.Who];
            if (collection == null)
                return;
            collection.Clear();
            Personal[args.Who] = null;
        }
        */
        #endregion

        #region GetAppliedTiles

        public static ITile[,] GetAppliedTiles(int X, int Y, int Width, int Height)
        {
            ITile[,] tiles = new ITile[Width, Height];
            int X2 = (X + Width), Y2 = (Y + Height);
            for (int i = X; i < X2; i++)
                for (int j = Y; j < Y2; j++)
                    tiles[i - X, j - Y] = Main.tile[i, j];

            for (int i = 0; i < Common.Order.Count; i++)
            {
                FakeTileRectangle fake = Common.Data[Common.Order[i]];
                if (fake.Enabled && fake.IsIntersecting(X, Y, Width, Height))
                    fake.ApplyTiles(tiles, X, Y);
            }
            /*
            for (int i = 0; i < Personal[Who].Order.Count; i++)
            {
                FakeTileRectangle fake = Personal[Who].Data[Personal[Who].Order[i]];
                if (fake.Enabled && fake.IsIntersecting(X, Y, Width, Height))
                    fake.ApplyTiles(tiles, X, Y);
            }
            */
            return tiles;
        }

        #endregion
        #region GetAppliedSigns

        public static Dictionary<int, Sign> GetAppliedSigns(int X, int Y, int Width, int Height)
        {
            Dictionary<int, Sign> signs = new Dictionary<int, Sign>();
            int X2 = (X + Width), Y2 = (Y + Height);
            for (int i = 0; i < Main.sign.Length; i++)
            {
                Sign sign = Main.sign[i];
                if ((sign != null) && (sign.x >= X) && (sign.x < X2)
                        && (sign.y >= Y) && (sign.y < Y2))
                    signs.Add(i, sign);
            }

            for (int i = 0; i < Common.Order.Count; i++)
            {
                FakeTileRectangle fake = Common.Data[Common.Order[i]];
                if (fake.Enabled && fake.IsIntersecting(X, Y, Width, Height))
                    fake.ApplySigns(signs, X, Y, Width, Height);
            }
            /*
            for (int i = 0; i < Personal[Who].Order.Count; i++)
            {
                FakeTileRectangle fake = Personal[Who].Data[Personal[Who].Order[i]];
                if (fake.Enabled && fake.IsIntersecting(X, Y, Width, Height))
                    fake.ApplySigns(signs, X, Y, Width, Height);
            }
            */
            return signs;
        }

        #endregion
    }
}