using System;
using FluentAssertions;
using SimpleMapper.Abstracts;
using SimpleMapper.Tests.Mocks;
using Xunit;

namespace SimpleMapper.Tests
{
    public class MapperTests
    {
        private IMapper _mapper;
        
        public MapperTests()
        {
            Mapper.Init();
            _mapper = Mapper.Instance;
        }
        
        [Fact]
        public void IsMapperMapSimpleModelCorrectly()
        {
            var expected = new ModelBMock().Model;
            
            var modelA = new ModelAMock().Model;
            
            ConfigureMap();

            var actual = _mapper.Map<ModelA, ModelB>(modelA);
            
            actual.Should().BeEquivalentTo(expected);
        }

        private void ConfigureMap()
        {
            Mapper.AddMap<ModelA, ModelB>(model=> new ModelB()
            {
                StringName = model.Name,
                IntNumber = model.Number,
                BooleanIsTrue = model.IsTrue
            });
        }
    }
}