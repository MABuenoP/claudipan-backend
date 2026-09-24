using AutoMapper;
using ClaudipanAPI.Models.DTOs;
using ClaudipanAPI.Models.Entities;

namespace ClaudipanAPI.Helpers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Producto
        CreateMap<Producto, ProductoDto>()
            .ForMember(dest => dest.CategoriaNombre,
                opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.Nombre : string.Empty));
        CreateMap<ProductoCreateDto, Producto>();
        CreateMap<ProductoUpdateDto, Producto>();

        // Categoria
        CreateMap<Categoria, CategoriaDto>();
        CreateMap<CategoriaCreateDto, Categoria>();
        CreateMap<CategoriaUpdateDto, Categoria>();

        // Proveedor
        CreateMap<Proveedor, ProveedorDto>();
        CreateMap<ProveedorCreateDto, Proveedor>();
        CreateMap<ProveedorUpdateDto, Proveedor>();

        // Insumo
        CreateMap<Insumo, InsumoDto>();
        CreateMap<InsumoCreateDto, Insumo>();
        CreateMap<InsumoUpdateDto, Insumo>();

        // Compra
        CreateMap<Compra, CompraDto>()
            .ForMember(dest => dest.ProveedorNombre, opt => opt.MapFrom(src => src.Proveedor != null ? src.Proveedor.Nombre : string.Empty));
        CreateMap<DetalleCompra, DetalleCompraDto>()
            .ForMember(dest => dest.InsumoNombre, opt => opt.MapFrom(src => src.Insumo != null ? src.Insumo.Nombre : string.Empty))
            .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty));

        // Receta
        CreateMap<RecetaProduccion, RecetaProduccionDto>()
            .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty));
        CreateMap<DetalleReceta, DetalleRecetaDto>()
            .ForMember(dest => dest.InsumoNombre, opt => opt.MapFrom(src => src.Insumo != null ? src.Insumo.Nombre : string.Empty))
            .ForMember(dest => dest.CostoUnitarioInsumo, opt => opt.MapFrom(src => src.Insumo != null ? src.Insumo.CostoUnitario : 0m));

        // Orden de Produccion
        CreateMap<OrdenProduccion, OrdenProduccionDto>()
            .ForMember(dest => dest.PanaderoNombre, opt => opt.MapFrom(src => src.PanaderoUsuario != null ? src.PanaderoUsuario.Nombre : string.Empty))
            .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty));

        // Gasto
        CreateMap<Gasto, GastoDto>()
            .ForMember(dest => dest.ResponsableUsuarioNombre, opt => opt.MapFrom(src => src.ResponsableUsuario != null ? src.ResponsableUsuario.Nombre : string.Empty));
        CreateMap<GastoCreateDto, Gasto>();

        // Baja
        CreateMap<BajaProducto, BajaProductoDto>()
            .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty))
            .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Nombre : string.Empty))
            .ForMember(dest => dest.EsParaTransformar, opt => opt.MapFrom(src => src.Motivo == "Transformacion" || (src.Observaciones != null && src.Observaciones.Contains("[TRANSFORMACIÓN"))));

        // Pedido
        CreateMap<Pedido, PedidoDto>()
            .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Nombre : (src.InvitadoNombre ?? "Comprador General")))
            .ForMember(dest => dest.ClienteEmail, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Email : (src.InvitadoEmail ?? "")))
            .ForMember(dest => dest.ClienteCedula, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Cedula : src.InvitadoCedula));
        CreateMap<DetallePedido, DetallePedidoDto>()
            .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty));

        // TransaccionDeuda
        CreateMap<TransaccionDeuda, TransaccionDeudaDto>()
            .ForMember(dest => dest.UsuarioNombre, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Nombre : string.Empty));

        // Auditoria
        CreateMap<Auditoria, AuditoriaDto>();

        // Usuario
        CreateMap<Usuario, UserProfileDto>();
        CreateMap<Usuario, UsuarioAdminDto>();
    }
}
