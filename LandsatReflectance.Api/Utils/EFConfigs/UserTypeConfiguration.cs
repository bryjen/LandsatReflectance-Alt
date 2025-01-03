﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LandsatReflectance.Api.Models;
using LandsatReflectance.Common.Models;

namespace LandsatReflectance.Backend.Utils.EFConfigs;

public class UserTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Guid);

        builder.Property(user => user.Guid)
            .HasColumnName("UserGuid")
            .IsRequired();

        builder.Property(user => user.Email)
            .HasColumnName("Email")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasColumnName("PasswordHash")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(user => user.IsEmailEnabled)
            .HasColumnName("EmailEnabled")
            .IsRequired();
    }
}