﻿using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using Microsoft.AspNetCore.Http;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs.Repository
{
    public class ChatHubContext : DBContextBase, IService
    {

        public virtual DbSet<ChatHubRoom> ChatHubRoom { get; set; }
        public virtual DbSet<ChatHubRoomChatHubUser> ChatHubRoomChatHubUser { get; set; }
        public virtual DbSet<ChatHubUser> ChatHubUser { get; set; }
        public virtual DbSet<ChatHubMessage> ChatHubMessage { get; set; }
        public virtual DbSet<ChatHubConnection> ChatHubConnection { get; set; }
        public virtual DbSet<ChatHubPhoto> ChatHubPhoto { get; set; }
        public virtual DbSet<ChatHubSetting> ChatHubSetting { get; set; }
        public virtual DbSet<ChatHubCam> ChatHubCam { get; set; }
        public virtual DbSet<ChatHubIgnore> ChatHubIgnore { get; set; }
        public virtual DbSet<ChatHubModerator> ChatHubModerator { get; set; }
        public virtual DbSet<ChatHubRoomChatHubModerator> ChatHubRoomChatHubModerator { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>().HasDiscriminator<string>("UserType").HasValue<User>("User").HasValue<ChatHubUser>("ChatHubUser");

            // Relations
            // Many-to-many
            // ChatHubRoom / ChatHubUser
            modelBuilder.Entity<ChatHubRoomChatHubUser>()
                .HasKey(item => new { item.ChatHubRoomId, item.ChatHubUserId });

            modelBuilder.Entity<ChatHubRoomChatHubUser>()
                .HasOne(room_user => room_user.Room)
                .WithMany(room => room.RoomUsers)
                .HasForeignKey(room_user => room_user.ChatHubRoomId);

            modelBuilder.Entity<ChatHubRoomChatHubUser>()
                .HasOne(room_user => room_user.User)
                .WithMany(user => user.UserRooms)
                .HasForeignKey(room_user => room_user.ChatHubUserId);

            // Relation
            // One-to-many
            // ChatHubMessage / ChatHubRoom
            modelBuilder.Entity<ChatHubMessage>()
                .HasOne(m => m.Room)
                .WithMany(r => r.Messages)
                .HasForeignKey(m => m.ChatHubRoomId);

            // Relation
            // One-to-many
            // ChatHubConnection / ChatHubUser
            modelBuilder.Entity<ChatHubConnection>()
                .HasOne(c => c.User)
                .WithMany(u => u.Connections)
                .HasForeignKey(c => c.ChatHubUserId);

            // Relation
            // One-to-many
            // ChatHubMessage / ChatHubPhotos
            modelBuilder.Entity<ChatHubPhoto>()
                .HasOne(p => p.Message)
                .WithMany(m => m.Photos)
                .HasForeignKey(p => p.ChatHubMessageId);

            // Relation
            // One-to-many
            // ChatHubUser / ChatHubIgnore
            modelBuilder.Entity<ChatHubIgnore>()
                .HasOne(i => i.User)
                .WithMany(u => u.Ignores)
                .HasForeignKey(i => i.ChatHubUserId);

            // Relation
            // One-to-one
            // ChatHubSetting / ChatHubUser
            modelBuilder.Entity<ChatHubSetting>()
                .HasOne(s => s.User)
                .WithOne(u => u.Settings)
                .HasForeignKey<ChatHubSetting>(s => s.ChatHubUserId);

            // Relation
            // One-to-one
            // ChatHubCam / ChatHubUser
            modelBuilder.Entity<ChatHubCam>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cam)
                .HasForeignKey<ChatHubCam>(c => c.ChatHubUserId);

            // Relations
            // Many-to-many
            // ChatHubRoom / ChatHubModerator
            modelBuilder.Entity<ChatHubRoomChatHubModerator>()
                .HasKey(item => new { item.ChatHubRoomId, item.ChatHubModeratorId });

            modelBuilder.Entity<ChatHubRoomChatHubModerator>()
                .HasOne(room_moderator => room_moderator.Room)
                .WithMany(room => room.RoomModerators)
                .HasForeignKey(room_moderator => room_moderator.ChatHubRoomId);

            modelBuilder.Entity<ChatHubRoomChatHubModerator>()
                .HasOne(room_moderator => room_moderator.Moderator)
                .WithMany(moderator => moderator.ModeratorRooms)
                .HasForeignKey(room_moderator => room_moderator.ChatHubModeratorId);


            base.OnModelCreating(modelBuilder);
        }

        public ChatHubContext(ITenantResolver tenantResolver, IHttpContextAccessor accessor) : base(tenantResolver, accessor)
        {
            // ContextBase handles multi-tenant database connections
        }

    }
}
