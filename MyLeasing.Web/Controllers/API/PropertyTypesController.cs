using System;
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
        [Route("GetPropertiesTypeWeb")]
        public async Task<IActionResult> GetPropertiesType()
        {
            try
            {
                var propertyTypes = await _dataContext.PropertyTypes.ToListAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Listado de los tipo de propiedades.",
                    Result = propertyTypes
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

    }
        
}