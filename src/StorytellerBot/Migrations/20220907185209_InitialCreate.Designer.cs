﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StorytellerBot.Data;

#nullable disable

namespace StorytellerBot.Migrations
{
    [DbContext(typeof(AdventureContext))]
    [Migration("20220907185209_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.8");

            modelBuilder.Entity("StorytellerBot.Models.Data.Adventure", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ScriptFileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Adventure");
                });

            modelBuilder.Entity("StorytellerBot.Models.Data.CommandProgress", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Argument")
                        .HasColumnType("TEXT");

                    b.Property<string>("Command")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Step")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("CommandProgress");
                });

            modelBuilder.Entity("StorytellerBot.Models.Data.CurrentGame", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SavedStatusId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId", "SavedStatusId");

                    b.HasIndex("SavedStatusId");

                    b.ToTable("CurrentGame");
                });

            modelBuilder.Entity("StorytellerBot.Models.Data.SavedStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AdventureId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("TEXT");

                    b.Property<string>("StoryState")
                        .HasColumnType("TEXT");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AdventureId");

                    b.HasIndex("UserId", "AdventureId")
                        .IsUnique();

                    b.ToTable("SavedStatus");
                });

            modelBuilder.Entity("StorytellerBot.Models.Data.User", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("StorytellerBot.Models.Data.CommandProgress", b =>
                {
                    b.HasOne("StorytellerBot.Models.Data.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("StorytellerBot.Models.Data.CurrentGame", b =>
                {
                    b.HasOne("StorytellerBot.Models.Data.SavedStatus", "SavedStatus")
                        .WithMany()
                        .HasForeignKey("SavedStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StorytellerBot.Models.Data.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SavedStatus");

                    b.Navigation("User");
                });

            modelBuilder.Entity("StorytellerBot.Models.Data.SavedStatus", b =>
                {
                    b.HasOne("StorytellerBot.Models.Data.Adventure", "Adventure")
                        .WithMany()
                        .HasForeignKey("AdventureId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StorytellerBot.Models.Data.User", "User")
                        .WithMany("SavedGames")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Adventure");

                    b.Navigation("User");
                });

            modelBuilder.Entity("StorytellerBot.Models.Data.User", b =>
                {
                    b.Navigation("SavedGames");
                });
#pragma warning restore 612, 618
        }
    }
}