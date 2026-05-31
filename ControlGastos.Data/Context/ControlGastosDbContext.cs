using System;
using System.Collections.Generic;
using ControlGastos.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Data.Context;

public partial class ControlGastosDbContext : DbContext
{
    public ControlGastosDbContext(DbContextOptions<ControlGastosDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Cuenta> Cuentas { get; set; }

    public virtual DbSet<FormasDePago> FormasDePagos { get; set; }

    public virtual DbSet<Movimiento> Movimientos { get; set; }

    public virtual DbSet<SaldosIniciale> SaldosIniciales { get; set; }

    public virtual DbSet<SubCategoria> SubCategorias { get; set; }

    public virtual DbSet<TiposMovimiento> TiposMovimientos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => e.NombreUsuario, "UQ_Usuarios_NombreUsuario").IsUnique();
            entity.Property(e => e.NombreUsuario).HasMaxLength(100);
            entity.Property(e => e.NombreCompleto).HasMaxLength(200);
            entity.Property(e => e.PasswordHash).HasMaxLength(100);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasIndex(e => e.Descripcion, "UQ_Categorias_Descripcion").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(150);
        });

        modelBuilder.Entity<Cuenta>(entity =>
        {
            entity.HasIndex(e => e.Descripcion, "UQ_Cuentas_Descripcion").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(150);
        });

        modelBuilder.Entity<FormasDePago>(entity =>
        {
            entity.ToTable("FormasDePago");

            entity.HasIndex(e => e.Descripcion, "UQ_FormasDePago_Descripcion").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(100);
        });

        modelBuilder.Entity<Movimiento>(entity =>
        {
            entity.HasIndex(e => new { e.IdCategoria, e.IdSubCategoria }, "IX_Movimientos_Categoria");

            entity.HasIndex(e => new { e.IdCuenta, e.FechaMovimiento }, "IX_Movimientos_Cuenta").IsDescending(false, true);

            entity.HasIndex(e => e.FechaMovimiento, "IX_Movimientos_Fecha").IsDescending();

            entity.Property(e => e.Detalle).HasMaxLength(500);
            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mov_Categoria");

            entity.HasOne(d => d.IdCuentaNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mov_Cuenta");

            entity.HasOne(d => d.IdFormaDePagoNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdFormaDePago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mov_FormaDePago");

            entity.HasOne(d => d.IdMovimientoVinculadoNavigation).WithMany(p => p.InverseIdMovimientoVinculadoNavigation)
                .HasForeignKey(d => d.IdMovimientoVinculado)
                .HasConstraintName("FK_Mov_MovimientoVinculado");

            entity.HasOne(d => d.IdSubCategoriaNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdSubCategoria)
                .HasConstraintName("FK_Mov_SubCategoria");

            entity.HasOne(d => d.IdTipoMovimientoNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdTipoMovimiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mov_TipoMovimiento");
        });

        modelBuilder.Entity<SaldosIniciale>(entity =>
        {
            entity.HasIndex(e => new { e.IdCuenta, e.Mes, e.Anio }, "UQ_SaldosIniciales_CuentaMesAnio").IsUnique();

            entity.Property(e => e.Monto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCuentaNavigation).WithMany(p => p.SaldosIniciales)
                .HasForeignKey(d => d.IdCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SaldosIniciales_Cuenta");
        });

        modelBuilder.Entity<SubCategoria>(entity =>
        {
            entity.HasIndex(e => new { e.Descripcion, e.IdCategoria }, "UQ_SubCat_DescCat").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(150);

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.SubCategoria)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubCat_Categoria");
        });

        modelBuilder.Entity<TiposMovimiento>(entity =>
        {
            entity.ToTable("TiposMovimiento");

            entity.HasIndex(e => e.Descripcion, "UQ_TiposMovimiento_Descripcion").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
