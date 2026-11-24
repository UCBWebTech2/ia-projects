using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SendEmail.Models;

namespace SendEmail.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Auditorium> Auditoria { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Documento> Documentos { get; set; }

    public virtual DbSet<Indice> Indices { get; set; }

    public virtual DbSet<Ocr> Ocrs { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<Proceso> Procesos { get; set; }
    public virtual DbSet<MapeoCategoria> MapeoCategorias { get; set; }
    public virtual DbSet<ModelRegistryEntry> ModelRegistryEntries { get; set; }
    public virtual DbSet<DocumentoEntidad> DocumentoEntidades { get; set; }
    public virtual DbSet<Busqueda> Busquedas { get; set; }
    public virtual DbSet<ClasificacionHistorial> ClasificacionHistoriales { get; set; }
    public virtual DbSet<Notificacion> Notificaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasKey(e => e.IdAuditoria).HasName("PK__Auditori__F1F30701C41C2577");

            entity.Property(e => e.IdAuditoria).HasColumnName("idAuditoria");
            entity.Property(e => e.Accion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("accion");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Auditoria__idUsu__31EC6D26");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__8A3D240C86A228BB");

            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.DescripcionCategoria)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcionCategoria");
            entity.Property(e => e.idUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.FechaCreacion)
                .HasColumnType("datetime")
                .HasColumnName("fechaCreacion");
            entity.Property(e => e.FechaModificacion)
                .HasColumnType("datetime")
                .HasColumnName("fechaModificacion");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombreCategoria");
            entity.Property(e => e.Habilitado)
                .HasColumnName("habilitado")
                .HasDefaultValue(true);
            entity.HasOne(d => d.UsuarioReferencia)
                    .WithMany(p => p.Categorias) // Usuario tiene una colección de Procesos
                    .HasForeignKey(d => d.idUsuario)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Categoria_Usuario");
            entity.HasMany(d => d.IdDocs).WithOne(p => p.CategoriaReferencia)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Documento_Categoria");


        });

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.HasKey(e => e.IdDoc).HasName("PK__Document__3E411446ABC716BA");

            entity.Property(e => e.IdDoc).HasColumnName("idDoc");
            entity.Property(e => e.ContenidoDocumento)
                .HasColumnName("contenidoDocumento")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.FechaCreacion)
                .HasColumnType("datetime")
                .HasColumnName("fechaCreacion");
            entity.Property(e => e.FechaModificacion)
                .HasColumnType("datetime")
                .HasColumnName("fechaModificacion");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.NombreDocumento)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("nombreDocumento");
            entity.Property(e => e.RutaDocumento)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaDocumento");
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipoDocumento");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Documentos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Documento__idUsu__267ABA7A");
            entity.Property(e => e.IdProceso)
                .HasColumnName("idProceso")
                .IsRequired(false);
            entity.Property(e => e.Habilitado)
                .HasColumnName("habilitado")
                .HasDefaultValue(true);
            entity.Property(e => e.PrediccionIa).HasMaxLength(100).IsUnicode(false).HasColumnName("prediccionIa");
            entity.Property(e => e.ConfianzaIa).HasColumnName("confianzaIa");
            entity.Property(e => e.VersionModelo).HasMaxLength(200).IsUnicode(false).HasColumnName("versionModelo");
            entity.Property(e => e.FechaPrediccion).HasColumnType("datetime").HasColumnName("fechaPrediccion");
            entity.Property(e => e.CategoriaConfirmadaId).HasColumnName("categoriaConfirmadaId");
            entity.Property(e => e.ConfirmadoPor).HasColumnName("confirmadoPor");
            entity.Property(e => e.FechaConfirmacion).HasColumnType("datetime").HasColumnName("fechaConfirmacion");
            entity.Property(e => e.AbogadoResponsableId).HasColumnName("abogadoResponsableId");
            entity.Property(e => e.ClienteNombre).HasMaxLength(200).HasColumnName("clienteNombre");
            entity.Property(e => e.EstadoDocumento).HasMaxLength(30).IsUnicode(false).HasColumnName("estadoDocumento").HasDefaultValue("ACTIVO");
            entity.Property(e => e.VersionActual).HasColumnName("versionActual").HasDefaultValue(1);
            entity.Property(e => e.HashContenido).HasMaxLength(128).IsUnicode(false).HasColumnName("hashContenido");
            entity.HasOne(d => d.ProcesoReferencia)
                .WithMany(p => p.IdDocs)
                .HasForeignKey(d => d.IdProceso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Documento_Proceso");
            entity.HasOne(d => d.CategoriaReferencia).WithMany(p => p.IdDocs)
            .HasForeignKey(d => d.IdCategoria)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Documento_Categoria");
        });
        modelBuilder.Entity<Indice>(entity =>
        {
            entity.HasKey(e => e.IdIndice).HasName("PK__Indices__D75E4057D3B2FDB5");
            entity.Property(e => e.IdIndice).HasColumnName("idIndice");
            entity.Property(e => e.Frecuencia).HasColumnName("frecuencia");
            entity.Property(e => e.IdDoc).HasColumnName("idDoc");
            entity.Property(e => e.Termino)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("termino");
            entity.Property(e => e.TipoTermino)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipoTermino");
            entity.HasOne(d => d.IdDocNavigation).WithMany(p => p.Indices)
                .HasForeignKey(d => d.IdDoc)
                .HasConstraintName("FK__Indices__idDoc__2F10007B");
        });

        modelBuilder.Entity<Ocr>(entity =>
        {
            entity.HasKey(e => e.IdOcr).HasName("PK__OCR__3D47D906C1B11B19");

            entity.ToTable("OCR");

            entity.Property(e => e.IdOcr).HasColumnName("idOCR");
            entity.Property(e => e.FechaModificacion)
                .HasColumnType("datetime")
                .HasColumnName("fechaModificacion");
            entity.Property(e => e.FechaProceso)
                .HasColumnType("datetime")
                .HasColumnName("fechaProceso");
            entity.Property(e => e.IdDoc).HasColumnName("idDoc");
            entity.Property(e => e.RutaDocumento)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("rutaDocumento");

            entity.HasOne(d => d.IdDocNavigation).WithMany(p => p.Ocrs)
                .HasForeignKey(d => d.IdDoc)
                .HasConstraintName("FK__OCR__idDoc__3F466844");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__645723A609FCBAA3");

            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Contrasenia)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasColumnName("contrasenia");
            entity.Property(e => e.CorreoElectronico)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("correoElectronico");
            entity.Property(e => e.FechaCreacion)
                .HasColumnType("datetime")
                .HasColumnName("fechaCreacion");
            entity.Property(e => e.FechaModificacion)
                .HasColumnType("datetime")
                .HasColumnName("fechaModificacion");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(70)
                .IsUnicode(false)
                .HasColumnName("nombreUsuario");
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("rol");
            entity.Property(e => e.Restablecer)
                .HasColumnName("restablecer");
            entity.Property(e => e.Confirmado)
                .HasColumnName("confirmar");
            entity.Property(e => e.Token)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasColumnName("Token");
        });
        modelBuilder.Entity<ModelRegistryEntry>(entity =>
        {
            entity.HasKey(e => e.IdModelRegistry);
            entity.ToTable("ModelRegistry");
            entity.Property(e => e.IdModelRegistry).HasColumnName("idModelRegistry");
            entity.Property(e => e.Version).HasMaxLength(100).IsUnicode(false).HasColumnName("version");
            entity.Property(e => e.ModelPath).HasMaxLength(500).IsUnicode(false).HasColumnName("modelPath");
            entity.Property(e => e.DatasetHash).HasMaxLength(128).IsUnicode(false).HasColumnName("datasetHash");
            entity.Property(e => e.DatasetPath).HasMaxLength(500).IsUnicode(false).HasColumnName("datasetPath");
            entity.Property(e => e.Accuracy).HasColumnName("accuracy");
            entity.Property(e => e.MacroF1).HasColumnName("macroF1");
            entity.Property(e => e.MetricsJson).HasColumnType("nvarchar(max)").HasColumnName("metricsJson");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime").HasColumnName("fechaCreacion");
            entity.Property(e => e.Notas).HasMaxLength(1000).HasColumnName("notas");
        });

        modelBuilder.Entity<MapeoCategoria>(entity =>
        {
            entity.HasKey(e => e.IdMapeoCategoria);
            entity.ToTable("MapeoCategoria");
            entity.Property(e => e.IdMapeoCategoria).HasColumnName("idMapeoCategoria");
            entity.Property(e => e.ModelClassName).HasMaxLength(100).IsUnicode(false).HasColumnName("modelClassName");
            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime").HasColumnName("fechaCreacion");
            entity.Property(e => e.FechaModificacion).HasColumnType("datetime").HasColumnName("fechaModificacion");
            entity.HasOne(d => d.Categoria).WithMany().HasForeignKey(d => d.IdCategoria).HasConstraintName("FK_MapeoCategoria_Categoria");
            entity.HasIndex(e => e.ModelClassName).IsUnique();
        });

        modelBuilder.Entity<Proceso>(entity =>
            {
                entity.HasKey(e => e.IdProceso).HasName("PK_Proceso");
                entity.Property(e => e.IdProceso).HasColumnName("idProceso");
                entity.Property(e => e.NombreProceso)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("nombreProceso");
                entity.Property(e => e.DescripcionProceso)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("descripcionProceso");
                entity.Property(e => e.idUsuario).HasColumnName("idUsuario");
                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("datetime")
                    .HasColumnName("fechaCreacion");
                entity.Property(e => e.FechaModificacion)
                    .HasColumnType("datetime")
                    .HasColumnName("fechaModificacion");
                entity.Property(e => e.Habilitado)
                .HasColumnName("habilitado")
                .HasDefaultValue(true);
                //entity.Property(e => e.Estado)
                //    .HasMaxLength(100)
                //    .IsUnicode(false)
                //    .HasColumnName("estado");
                entity.HasOne(d => d.UsuarioReferencia)
                    .WithMany(p => p.Procesos)
                    .HasForeignKey(d => d.idUsuario)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Proceso_Usuario");

                entity.HasMany(d => d.IdDocs)
                    .WithOne(p => p.ProcesoReferencia)
                    .HasForeignKey(d => d.IdProceso)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Documento_Proceso");
            });

        modelBuilder.Entity<DocumentoEntidad>(entity =>
        {
            entity.HasKey(e => e.IdEntidad);
            entity.ToTable("DocumentoEntidades");
            entity.Property(e => e.IdEntidad).HasColumnName("idEntidad");
            entity.Property(e => e.IdDoc).HasColumnName("idDoc");
            entity.Property(e => e.TipoEntidad).HasMaxLength(50).IsUnicode(false).HasColumnName("tipoEntidad");
            entity.Property(e => e.Valor).HasMaxLength(300).HasColumnName("valor");
            entity.Property(e => e.ValorNormalizado).HasMaxLength(300).HasColumnName("valorNormalizado");
            entity.Property(e => e.Fuente).HasMaxLength(30).IsUnicode(false).HasColumnName("fuente").HasDefaultValue("NER");
            entity.Property(e => e.Confianza).HasColumnName("confianza");
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime").HasColumnName("fechaCreacion");
            entity.HasOne(d => d.IdDocNavigation).WithMany(p => p.DocumentoEntidades)
                .HasForeignKey(d => d.IdDoc).HasConstraintName("FK_DocumentoEntidades_Doc");
            entity.HasIndex(e => new { e.IdDoc, e.TipoEntidad });
            entity.HasIndex(e => new { e.TipoEntidad, e.ValorNormalizado });
        });

        modelBuilder.Entity<Busqueda>(entity =>
        {
            entity.HasKey(e => e.IdBusqueda);
            entity.ToTable("Busquedas");
            entity.Property(e => e.IdBusqueda).HasColumnName("idBusqueda");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.TipoBusqueda).HasMaxLength(50).IsUnicode(false).HasColumnName("tipoBusqueda");
            entity.Property(e => e.Query).HasMaxLength(1000).HasColumnName("query");
            entity.Property(e => e.FiltrosJson).HasColumnType("nvarchar(max)").HasColumnName("filtrosJson");
            entity.Property(e => e.Resultados).HasColumnName("resultados");
            entity.Property(e => e.TiempoMs).HasColumnName("tiempoMs");
            entity.Property(e => e.Fecha).HasColumnType("datetime").HasColumnName("fecha");
            entity.HasOne(d => d.IdUsuarioNavigation).WithMany().HasForeignKey(d => d.IdUsuario).HasConstraintName("FK_Busquedas_Usuario");
            entity.HasIndex(e => e.Fecha);
            entity.HasIndex(e => new { e.IdUsuario, e.Fecha });
        });

        modelBuilder.Entity<ClasificacionHistorial>(entity =>
        {
            entity.HasKey(e => e.IdHistorial);
            entity.ToTable("ClasificacionHistorial");
            entity.Property(e => e.IdHistorial).HasColumnName("idHistorial");
            entity.Property(e => e.IdDoc).HasColumnName("idDoc");
            entity.Property(e => e.CategoriaAnteriorId).HasColumnName("categoriaAnteriorId");
            entity.Property(e => e.CategoriaNuevaId).HasColumnName("categoriaNuevaId");
            entity.Property(e => e.PrediccionIa).HasMaxLength(100).IsUnicode(false).HasColumnName("prediccionIa");
            entity.Property(e => e.ConfianzaIa).HasColumnName("confianzaIa");
            entity.Property(e => e.VersionModelo).HasMaxLength(200).IsUnicode(false).HasColumnName("versionModelo");
            entity.Property(e => e.Origen).HasMaxLength(30).IsUnicode(false).HasColumnName("origen");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Fecha).HasColumnType("datetime").HasColumnName("fecha");
            entity.Property(e => e.Comentario).HasMaxLength(500).HasColumnName("comentario");
            entity.HasOne(d => d.IdDocNavigation).WithMany().HasForeignKey(d => d.IdDoc).HasConstraintName("FK_ClasificacionHistorial_Doc");
            entity.HasOne(d => d.IdUsuarioNavigation).WithMany().HasForeignKey(d => d.IdUsuario).HasConstraintName("FK_ClasificacionHistorial_Usuario");
            entity.HasIndex(e => new { e.IdDoc, e.Fecha });
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion);
            entity.ToTable("Notificaciones");
            entity.Property(e => e.IdNotificacion).HasColumnName("idNotificacion");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Tipo).HasMaxLength(50).IsUnicode(false).HasColumnName("tipo");
            entity.Property(e => e.Titulo).HasMaxLength(200).HasColumnName("titulo");
            entity.Property(e => e.Mensaje).HasMaxLength(1000).HasColumnName("mensaje");
            entity.Property(e => e.Leida).HasColumnName("leida");
            entity.Property(e => e.EnviadaCorreo).HasColumnName("enviadaCorreo");
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime").HasColumnName("fechaCreacion");
            entity.Property(e => e.MetadataJson).HasColumnType("nvarchar(max)").HasColumnName("metadataJson");
            entity.HasOne(d => d.IdUsuarioNavigation).WithMany().HasForeignKey(d => d.IdUsuario).HasConstraintName("FK_Notificaciones_Usuario");
            entity.HasIndex(e => new { e.IdUsuario, e.Leida, e.FechaCreacion });
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
