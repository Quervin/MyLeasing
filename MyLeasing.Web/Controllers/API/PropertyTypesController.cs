﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLeasing.Common.Models;
using MyLeasing.Web.Data;
using MyLeasing.Web.Data.Entities;
using MyLeasing.Web.Models;

namespace MyLeasing.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PropertyTypesController : ControllerBase
    {
        private readonly DataContext _dataContext;

        public PropertyTypesController(DataContext context)
        {
            _dataContext = context;
        }

        [HttpGet]
        public IEnumerable<PropertyType> GetPropertyTypes()
        {
            return _dataContext.PropertyTypes.OrderBy(pt => pt.Name);
        }

        //Métodos para Angular
        [HttpGet]
        [Route("GetPropertiesTypeWeb/{index}/{countPages}")]
        public async Task<IActionResult> GetPropertiesType(int index, int countPages)
        {
            try
            {
                var total = await _dataContext.PropertyTypes.CountAsync();

                var propertyTypes = await _dataContext.PropertyTypes.Select(x => new PropertyTypeResponseApi()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Properties = x.Properties != null ? toPropertiesResponseApi(x.Properties) : new List<PropertyResponseApi>(),
                }).Skip(index).Take(countPages).ToListAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Listado de los tipo de propiedades.",
                    Result = propertyTypes,
                    Total = total
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intentar obtener el listado de los tipos de propiedades." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DetailsPropertiesTypeWeb/{propertyTypeId}")]
        public async Task<IActionResult> DetailsPropertiesType(int propertyTypeId)
        {
            try
            {
                var propertyType = await _dataContext.PropertyTypes.Include(o => o.Properties).Select(x => new PropertyTypeResponseApi()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Properties = x.Properties != null ? toPropertiesResponseApi(x.Properties) : new List<PropertyResponseApi>(),
                }).FirstOrDefaultAsync(m => m.Id == propertyTypeId);

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Listado de los tipo de propiedades.",
                    Result = propertyType
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intentar obtener el listado de los tipos de propiedades." + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("CreateWeb")]
        public async Task<IActionResult> Create(AddPropertyTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al intertar agregar el tipo de propiedad."
                    });
                }

                var propertyType = new PropertyType
                {
                    Name = request.Name
                };

                _dataContext.PropertyTypes.Add(propertyType);
                await _dataContext.SaveChangesAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "El tipo de propiedad ha sido agregado correctamente."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intertar agregar el tipo de propiedad." + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("EditWeb")]
        public async Task<IActionResult> Edit(AddPropertyTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al intertar editar el tipo de propiedad."
                    });
                }

                var propertyType = new PropertyType
                {
                    Name = request.Name
                };

                _dataContext.PropertyTypes.Update(propertyType);
                await _dataContext.SaveChangesAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "El tipo de propiedad ha sido actualizado correctamente."
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intertar editar el tipo de propiedad." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("DeleteWeb/{propertieId}")]
        public async Task<IActionResult> Delete(int propertyTypeId)
        {
            try
            {
                var propertyType = await _dataContext.PropertyTypes
                .Include(pt => pt.Properties)
                .FirstOrDefaultAsync(m => m.Id == propertyTypeId);

                if (propertyType == null)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "Se ha producido un error al eliminar el tipo de propiedad."
                    });
                }

                if (propertyType.Properties.Count > 0)
                {
                    return Ok(new Response<object>
                    {
                        IsSuccess = false,
                        Message = "No se puede eliminar el tipo de propiedad porque existen propiedades con este tipo de propiedad.",
                    });
                }

                _dataContext.PropertyTypes.Remove(propertyType);
                await _dataContext.SaveChangesAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Se elimino el tipo de propiedad correctamente.",
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al eliminar el tipo de propiedad." + ex.Message
                });
            }
        }

        private List<PropertyResponseApi> toPropertiesResponseApi(ICollection<Property> properties)
        {
            return properties.Select(x => new PropertyResponseApi()
            {
                Id = x.Id,
                Neighborhood = x.Neighborhood,
                Address = x.Address,
                Price = x.Price,
                SquareMeters = x.SquareMeters,
                Rooms = x.Rooms,
                Stratum = x.Stratum,
                HasParkingLot = x.HasParkingLot,
                IsAvailable = x.IsAvailable,
                Remarks = x.Remarks,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                PropertyType = x.PropertyType != null ? new PropertyTypeResponseApi()
                {
                    Id = x.PropertyType.Id,
                    Name = x.PropertyType.Name
                } : new PropertyTypeResponseApi(),
                Owner = x.Owner != null ? new OwnerResponseApi()
                {
                    Id = x.Owner.Id,
                    User = new UserResponseApi()
                    {
                        Document = x.Owner.User.Document,
                        Address = x.Owner.User.Address,
                        FirstName = x.Owner.User.FirstName,
                        LastName = x.Owner.User.LastName
                    }
                } : new OwnerResponseApi(),
                PropertyImages = x.PropertyImages != null ? toPropertyImageResponseApi(x.PropertyImages) : new List<PropertyImageResponseApi>(),
                Contracts = x.Contracts != null ? toContactsResponseApi(x.Contracts) : new List<ContractResponseApi>()
            }).ToList();
        }

        private List<PropertyImageResponseApi> toPropertyImageResponseApi(ICollection<PropertyImage> propertyImages)
        {
            return propertyImages.Select(x => new PropertyImageResponseApi()
            {
                Id = x.Id,
                ImageUrl = x.ImageUrl
            }).ToList();
        }

        private List<ContractResponseApi> toContactsResponseApi(ICollection<Contract> contracts)
        {
            return contracts.Select(x => new ContractResponseApi()
            {
                Id = x.Id,
                EndDate = x.EndDate,
                IsActive = x.IsActive,
                StartDate = x.StartDate,
                Remarks = x.Remarks,
                Price = x.Price,
                Owner = x.Owner != null ? new OwnerResponseApi()
                {
                    Id = x.Owner.Id,
                    User = new UserResponseApi()
                    {
                        Document = x.Owner.User.Document,
                        Address = x.Owner.User.Address,
                        FirstName = x.Owner.User.FirstName,
                        LastName = x.Owner.User.LastName
                    }
                } : new OwnerResponseApi(),
                Lessee = x.Lessee != null ? new LesseeResponseApi()
                {
                    Id = x.Lessee.Id,
                    User = new UserResponseApi()
                    {
                        Document = x.Owner.User.Document,
                        Address = x.Owner.User.Address,
                        FirstName = x.Owner.User.FirstName,
                        LastName = x.Owner.User.LastName
                    }
                } : new LesseeResponseApi()
            }).ToList();
        }

    }

}