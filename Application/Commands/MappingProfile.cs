using Application.Commands.AddTransaction;
using Application.Requests;
using AutoMapper;

namespace Application.Commands;

public class MappingProfile : Profile
{
  public MappingProfile()
  {
    CreateMap<AddTransactionRequest,AddTransactionCommand>();
    CreateMap<CreateSavingsAccountRequest,CreateSavingsAccountCommand>();
  }
}