﻿using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using Entities.Models;
using System;
using System.Collections.Generic;

namespace SchoolAPI.Controllers
{
    [Route("api/v1/assignments")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")]
    public class AssignmentsController : CommonProperties
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public AssignmentsController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet(Name = "getAllAssignments")]
        public override IActionResult Get()
        {
            var assignments = _repository.Assignment.GetAllAssignments(trackChanges: false);

            var assignmentDto = _mapper.Map<IEnumerable<AssignmentDto>>(assignments);
            //uncomment the code below to test the global exception handling
            //throw new Exception("Exception");
            return Ok(assignmentDto);
        }

        [HttpGet("{id}", Name = "getAssignmentById")]
        public override IActionResult Get(Guid id)
        {
            var assignment = _repository.Assignment.GetAssignment(id, trackChanges: false); if (assignment == null)
            {
                _logger.LogInfo($"Assignment with id: {id} doesn't exist in the database.");
                return NotFound();
            }
            else
            {
                var assignmentDto = _mapper.Map<AssignmentDto>(assignment);
                return Ok(assignmentDto);
            }
        }

        [HttpPost(Name = "createAssignment")]
        public override IActionResult Create([FromBody] CreateItem item)
        {
            if (item == null)
            {
                _logger.LogError("Assignment ForCreationDto object sent from client is null.");
                return BadRequest("Assignment ForCreationDto object is null");
            }
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the AssignmentForCreationDto object");
                return UnprocessableEntity(ModelState);
            }

            var assignmentEntity = _mapper.Map<Assignment>(item);

            _repository.Assignment.CreateAssignment(assignmentEntity);
            _repository.Save();

            var assignmentToReturn = _mapper.Map<AssignmentDto>(assignmentEntity);

            return CreatedAtRoute("getAssignmentById", new { id = assignmentToReturn.Id }, assignmentToReturn);
        }

        [HttpPut("{id}")]
        public override IActionResult Update(Guid id, [FromBody] NameString str)
        {
            if (str == null)
            {
                _logger.LogError("AssignmentForUpdateDto object sent from client is null.");
                return BadRequest("AssignmentForUpdateDto object is null");
            }
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the AssignmentForUpdateDto object");
                return UnprocessableEntity(ModelState);
            }
            var assignmentEntity = _repository.Assignment.GetAssignment(id, trackChanges: true);
            if (assignmentEntity == null)
            {
                _logger.LogInfo($"Assignment with id: {id} doesn't exist in the database.");
                return NotFound();
            }

            _mapper.Map(str, assignmentEntity);
            _repository.Save();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public override IActionResult Delete(Guid id)
        {
            var assignment = _repository.Assignment.GetAssignment(id, trackChanges: false);
            if (assignment == null)
            {
                _logger.LogInfo($"Assignment with id: {id} doesn't exist in the database.");
                return NotFound();
            }

            _repository.Assignment.DeleteAssignment(assignment);
            _repository.Save();

            return NoContent();
        }
    }
}