using AutoMapper;
using SharedModels;
using SharedModels.Clases;
using SharedModels.Dto;



namespace TritoteNic
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            // Usuario
            CreateMap<Usuario, UsuarioDto>()
                .ForMember(dest => dest.NombreRol, opt => opt.MapFrom(src => src.Rol != null ? src.Rol.NombreRol : null))
                .ReverseMap()
                .ForMember(dest => dest.Rol, opt => opt.Ignore());
            CreateMap<Usuario, UsuarioCreateDto>().ReverseMap()
                .ForMember(dest => dest.Rol, opt => opt.Ignore());
            CreateMap<Usuario, UsuarioUpdateDto>().ReverseMap()
                .ForMember(dest => dest.Rol, opt => opt.Ignore());

            // Producto
            CreateMap<Producto, ProductoDto>()
                .ForMember(dest => dest.NombreCategoria, opt => opt.MapFrom(src => src.Categoria != null ? src.Categoria.NombreCategoria : null))
                .ReverseMap()
                .ForMember(dest => dest.Categoria, opt => opt.Ignore());
            CreateMap<Producto, ProductoCreateDto>().ReverseMap()
                .ForMember(dest => dest.Categoria, opt => opt.Ignore());
            CreateMap<Producto, ProductoUpdateDto>().ReverseMap()
                .ForMember(dest => dest.Categoria, opt => opt.Ignore());

            // Cliente - Los campos calculados (TotalGastado, TotalPedidos, FechaUltimoPedido) 
            // se calculan en el controlador
            CreateMap<Cliente, ClienteDto>().ReverseMap();
            CreateMap<Cliente, ClienteCreateDto>().ReverseMap();
            CreateMap<Cliente, ClienteUpdateDto>().ReverseMap();

            // Pedido - Los campos de nombres (NombreCliente, NombreUsuario, etc.) 
            // y Detalles se mapean manualmente en el controlador
            CreateMap<Pedido, PedidoDto>()
                .ForMember(dest => dest.NombreCliente, opt => opt.Ignore())
                .ForMember(dest => dest.NombreUsuario, opt => opt.Ignore())
                .ForMember(dest => dest.NombreEstadoPedido, opt => opt.Ignore())
                .ForMember(dest => dest.NombreMetodoPago, opt => opt.Ignore())
                .ForMember(dest => dest.Detalles, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Cliente, opt => opt.Ignore())
                .ForMember(dest => dest.Usuario, opt => opt.Ignore())
                .ForMember(dest => dest.EstadoPedido, opt => opt.Ignore())
                .ForMember(dest => dest.MetodoPago, opt => opt.Ignore())
                .ForMember(dest => dest.Detalles, opt => opt.Ignore());
            CreateMap<Pedido, PedidoCreateDto>().ReverseMap()
                .ForMember(dest => dest.Cliente, opt => opt.Ignore())
                .ForMember(dest => dest.Usuario, opt => opt.Ignore())
                .ForMember(dest => dest.EstadoPedido, opt => opt.Ignore())
                .ForMember(dest => dest.MetodoPago, opt => opt.Ignore())
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));
            CreateMap<Pedido, PedidoUpdateDto>().ReverseMap()
                .ForMember(dest => dest.Cliente, opt => opt.Ignore())
                .ForMember(dest => dest.Usuario, opt => opt.Ignore())
                .ForMember(dest => dest.EstadoPedido, opt => opt.Ignore())
                .ForMember(dest => dest.MetodoPago, opt => opt.Ignore())
                .ForMember(dest => dest.Detalles, opt => opt.Ignore());

            // DetallePedido
            CreateMap<DetallePedido, DetallePedidoDto>()
                .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.NombreProducto : null))
                .ReverseMap()
                .ForMember(dest => dest.Producto, opt => opt.Ignore())
                .ForMember(dest => dest.Pedido, opt => opt.Ignore());
            CreateMap<DetallePedido, DetallePedidoCreateDto>().ReverseMap()
                .ForMember(dest => dest.Producto, opt => opt.Ignore())
                .ForMember(dest => dest.Pedido, opt => opt.Ignore());
            CreateMap<DetallePedido, DetallePedidoUpdateDto>().ReverseMap()
                .ForMember(dest => dest.Producto, opt => opt.Ignore())
                .ForMember(dest => dest.Pedido, opt => opt.Ignore());

            // Categoria
            CreateMap<Categoria, CategoriaDto>().ReverseMap();
            CreateMap<Categoria, CategoriaCreateDto>().ReverseMap();
            CreateMap<Categoria, CategoriaUpdateDto>().ReverseMap();

            // MetodoPago
            CreateMap<MetodoPago, MetodoPagoDto>().ReverseMap();
            CreateMap<MetodoPago, MetodoPagoCreateDto>().ReverseMap();
            CreateMap<MetodoPago, MetodoPagoUpdateDto>().ReverseMap();

            // EstadoPedido
            CreateMap<EstadoPedido, EstadoPedidoDto>().ReverseMap();
            CreateMap<EstadoPedido, EstadoPedidoCreateDto>().ReverseMap();
            CreateMap<EstadoPedido, EstadoPedidoUpdateDto>().ReverseMap();

            // Rol
            CreateMap<Rol, RolDto>().ReverseMap();
            CreateMap<Rol, RolCreateDto>().ReverseMap();
            CreateMap<Rol, RolUpdateDto>().ReverseMap();
        }
    }
}
