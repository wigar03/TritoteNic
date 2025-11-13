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
            CreateMap<Usuario, UsuarioDto>().ReverseMap();
            CreateMap<Usuario, UsuarioCreateDto>().ReverseMap();
            CreateMap<Usuario, UsuarioUpdateDto>().ReverseMap();
            CreateMap<Producto, ProductoDto>().ReverseMap();
            CreateMap<Producto, ProductoCreateDto>().ReverseMap();
            CreateMap<Producto, ProductoUpdateDto>().ReverseMap();
            CreateMap<Cliente, ClienteDto>().ReverseMap();
            CreateMap<Cliente, ClienteCreateDto>().ReverseMap();
            CreateMap<Cliente, ClienteUpdateDto>().ReverseMap();
            CreateMap<Pedido, PedidoDto>().ReverseMap();
            CreateMap<Pedido, PedidoCreateDto>().ReverseMap();
            CreateMap<Pedido, PedidoUpdateDto>().ReverseMap();
            CreateMap<DetallePedido, DetallePedidoDto>().ReverseMap();
            CreateMap<DetallePedido, DetallePedidoCreateDto>().ReverseMap();
            CreateMap<DetallePedido, DetallePedidoUpdateDto>().ReverseMap();
            CreateMap<Categoria, CategoriaDto>().ReverseMap();
            CreateMap<Categoria, CategoriaCreateDto>().ReverseMap();
            CreateMap<Categoria, CategoriaUpdateDto>().ReverseMap();
            CreateMap<MetodoPago, MetodoPagoDto>().ReverseMap();
            CreateMap<MetodoPago, MetodoPagoCreateDto>().ReverseMap();
            CreateMap<MetodoPago, MetodoPagoUpdateDto>().ReverseMap();
            CreateMap<EstadoPedido, EstadoPedidoDto>().ReverseMap();
            CreateMap<EstadoPedido, EstadoPedidoCreateDto>().ReverseMap();
            CreateMap<EstadoPedido, EstadoPedidoUpdateDto>().ReverseMap();
            CreateMap<Rol, RolDto>().ReverseMap();
            CreateMap<Rol, RolCreateDto>().ReverseMap();
            CreateMap<Rol, RolUpdateDto>().ReverseMap();

        }
    }
}
