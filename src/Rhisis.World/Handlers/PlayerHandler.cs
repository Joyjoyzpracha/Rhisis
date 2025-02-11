﻿using Ether.Network.Packets;
using Microsoft.Extensions.Logging;
using Rhisis.Core.Data;
using Rhisis.Core.DependencyInjection;
using Rhisis.Core.Structures;
using Rhisis.Network;
using Rhisis.Network.Packets;
using Rhisis.Network.Packets.World;
using Rhisis.World.Game.Maps.Regions;
using Rhisis.World.Packets;
using Rhisis.World.Systems.Death;
using Rhisis.World.Systems.Follow;
using Rhisis.World.Systems.PlayerData;
using Rhisis.World.Systems.PlayerData.EventArgs;
using Rhisis.World.Systems.SpecialEffect;
using Rhisis.World.Systems.SpecialEffect.EventArgs;
using System;

namespace Rhisis.World.Handlers
{
    internal class PlayerHandler
    {
        private static readonly ILogger Logger = DependencyContainer.Instance.Resolve<ILogger<PlayerHandler>>();

        [PacketHandler(PacketType.PLAYERSETDESTOBJ)]
        public static void OnPlayerSetDestObject(WorldClient client, INetPacketStream packet)
        {
            var targetObjectId = packet.Read<uint>();
            var distance = packet.Read<float>();
            var followEvent = new FollowEventArgs(targetObjectId, distance);

            // Cancel current item usage action and SFX
            client.Player.NotifySystem<SpecialEffectSystem>(new SpecialEffectBaseMotionEventArgs(StateModeBaseMotion.BASEMOTION_OFF));
            client.Player.Delayer.CancelAction(client.Player.Inventory.ItemInUseActionId);
            client.Player.Inventory.ItemInUseActionId = Guid.Empty;

            client.Player.NotifySystem<FollowSystem>(followEvent);
        }

        [PacketHandler(PacketType.QUERY_PLAYER_DATA)]
        public static void OnQueryPlayerData(WorldClient client, INetPacketStream packet)
        {
            var onQueryPlayerDataPacket = new QueryPlayerDataPacket(packet);
            var queryPlayerDataEvent = new QueryPlayerDataEventArgs(onQueryPlayerDataPacket.PlayerId, onQueryPlayerDataPacket.Version);
            client.Player.NotifySystem<PlayerDataSystem>(queryPlayerDataEvent);
        }

        [PacketHandler(PacketType.QUERY_PLAYER_DATA2)]
        public static void OnQueryPlayerData2(WorldClient client, INetPacketStream packet)
        {
            var onQueryPlayerData2Packet = new QueryPlayerData2Packet(packet);
            var queryPlayerData2Event = new QueryPlayerData2EventArgs(onQueryPlayerData2Packet.Size, onQueryPlayerData2Packet.PlayerDictionary);
            client.Player.NotifySystem<PlayerDataSystem>(queryPlayerData2Event);
        }

        [PacketHandler(PacketType.PLAYERMOVED)]
        public static void OnPlayerMoved(WorldClient client, INetPacketStream packet)
        {
            var playerMovedPacket = new PlayerMovedPacket(packet);

            if (client.Player.Health.IsDead)
            {
                Logger.LogError($"Player {client.Player.Object.Name} is dead, he cannot move with keyboard.");
                return;
            }

            // Cancel current item usage action and SFX
            client.Player.NotifySystem<SpecialEffectSystem>(new SpecialEffectBaseMotionEventArgs(StateModeBaseMotion.BASEMOTION_OFF));
            client.Player.Delayer.CancelAction(client.Player.Inventory.ItemInUseActionId);
            client.Player.Inventory.ItemInUseActionId = Guid.Empty;

            // TODO: Check if player is flying

            client.Player.Follow.Reset();
            client.Player.Battle.Reset();
            client.Player.Object.Position = playerMovedPacket.BeginPosition + playerMovedPacket.DestinationPosition;
            client.Player.Object.Angle = playerMovedPacket.Angle;
            client.Player.Object.MovingFlags = (ObjectState)playerMovedPacket.State;
            client.Player.Object.MotionFlags = (StateFlags)playerMovedPacket.StateFlag;
            client.Player.Moves.IsMovingWithKeyboard = client.Player.Object.MovingFlags.HasFlag(ObjectState.OBJSTA_FMOVE) || 
                client.Player.Object.MovingFlags.HasFlag(ObjectState.OBJSTA_BMOVE);
            client.Player.Moves.DestinationPosition = playerMovedPacket.BeginPosition + playerMovedPacket.DestinationPosition;

            WorldPacketFactory.SendMoverMoved(client.Player,
                playerMovedPacket.BeginPosition, 
                playerMovedPacket.DestinationPosition,
                client.Player.Object.Angle, 
                (uint)client.Player.Object.MovingFlags, 
                (uint)client.Player.Object.MotionFlags, 
                playerMovedPacket.Motion, 
                playerMovedPacket.MotionEx, 
                playerMovedPacket.Loop, 
                playerMovedPacket.MotionOption, 
                playerMovedPacket.TickCount);
        }

        [PacketHandler(PacketType.PLAYERBEHAVIOR)]
        public static void OnPlayerBehavior(WorldClient client, INetPacketStream packet)
        {
            var playerBehaviorPacket = new PlayerBehaviorPacket(packet);

            if (client.Player.Health.IsDead)
            {
                Logger.LogError($"Player {client.Player.Object.Name} is dead, he cannot move with keyboard.");
                return;
            }

            // TODO: check if player is flying

            client.Player.Object.Position = playerBehaviorPacket.BeginPosition + playerBehaviorPacket.DestinationPosition;
            client.Player.Object.Angle = playerBehaviorPacket.Angle;
            client.Player.Object.MovingFlags = (ObjectState)playerBehaviorPacket.State;
            client.Player.Object.MotionFlags = (StateFlags)playerBehaviorPacket.StateFlag;
            client.Player.Moves.IsMovingWithKeyboard = client.Player.Object.MovingFlags.HasFlag(ObjectState.OBJSTA_FMOVE) ||
                client.Player.Object.MovingFlags.HasFlag(ObjectState.OBJSTA_BMOVE);
            client.Player.Moves.DestinationPosition = playerBehaviorPacket.BeginPosition + playerBehaviorPacket.DestinationPosition;

            WorldPacketFactory.SendMoverBehavior(client.Player,
                playerBehaviorPacket.BeginPosition,
                playerBehaviorPacket.DestinationPosition,
                client.Player.Object.Angle,
                (uint)client.Player.Object.MovingFlags,
                (uint)client.Player.Object.MotionFlags,
                playerBehaviorPacket.Motion,
                playerBehaviorPacket.MotionEx,
                playerBehaviorPacket.Loop,
                playerBehaviorPacket.MotionOption,
                playerBehaviorPacket.TickCount);
        }

        [PacketHandler(PacketType.REVIVAL_TO_LODESTAR)]
        public static void OnRevivalToLodestar(WorldClient client, INetPacketStream _)
        {
            if (!client.Player.Health.IsDead)
            {
                Logger.LogWarning($"Player '{client.Player.Object.Name}' tried to revival to lodestar without being dead.");
                return;
            }

            client.Player.NotifySystem<DeathSystem>();
        }
    }
}
