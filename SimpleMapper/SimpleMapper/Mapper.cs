using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using SimpleMapper.Abstracts;

namespace SimpleMapper
{
    public class Mapper: IMapper
    {
        private static readonly object Locker = new object(); 
        private static Dictionary<Type, Dictionary<Type, Delegate>> _maps = new Dictionary<Type, Dictionary<Type, Delegate>>();

        private static volatile bool _isInit;

        public Mapper() { }
        private static readonly Lazy<Mapper> Lazy = new Lazy<Mapper>(()=>new Mapper());
        public static IMapper Instance => Lazy?.Value;
        
        public static void Init()
        {
            lock (Locker)
            {
                if (_isInit) return;
                
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(GetLoadableTypes)
                    .Where(type => typeof(IMapperConfig).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                    .ToList()
                    .ForEach(type => (Activator.CreateInstance(type) as IMapperConfig)?.AddMaps());
            }
        }
        public TResult Map<TIn, TResult>(TIn model)
        {
            if(model == null)
                return default;

            var mapFunction = Get<TIn, TResult, Func<TIn, TResult>>();

            if (mapFunction == null)
                return default;

            return mapFunction(model);
        }

        public IEnumerable<TResult> Map<TIn, TResult>(IEnumerable<TIn> models)
        {
            if(models == null)
                return default;

            var mapFunction = Get<TIn, TResult, Func<TIn, TResult>>();

            if (mapFunction == null)
                return default;

            return models.Select(model => mapFunction(model));
        }

        public static void AddMap<TIn, TResult>(Func<TIn, TResult> mapFunction)
        {
            AddMapFunction<TIn, TResult>(mapFunction);
        }

        private static void AddMapFunction<TIn, TResult>(Delegate mapFunction)
        {
            if(mapFunction == null)
                throw new ArgumentNullException(nameof(mapFunction));

            lock (Locker)
            {
                if(!_maps.ContainsKey(typeof(TIn)))
                    _maps.Add(typeof(TIn), new Dictionary<Type, Delegate>());

                var typeMaps = _maps[typeof(TIn)];
                
                if(typeMaps.ContainsKey(typeof(TResult)))
                    throw new InvalidOperationException($"Правило для преобразования {typeof(TIn)} в {typeof(TResult)} уже зарегистрировано");
                
                typeMaps.Add(typeof(TResult), mapFunction);
            }
        }

        private TMapFunction Get<TIn, TResult, TMapFunction>()
        where TMapFunction: class
        {
            if(!_maps.TryGetValue(typeof(TIn), out var from))
                throw new NotImplementedException($"Не установлено правило преобразования объекта типа {typeof(TIn)} в объект типа {typeof(TResult)}");
            if(!from.TryGetValue(typeof(TResult), out var result))
                throw new NotImplementedException($"Не установлено правило преобразования объекта типа {typeof(TIn)} в объект типа {typeof(TResult)}");

            var mapFunction = result as TMapFunction;

            if (mapFunction == null)
            {
                throw new Exception($"Тип результата- {result.GetType()}, тип функции - {typeof(TMapFunction)}");
            }

            return mapFunction;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            try
            {
                var c = assembly.GetTypes();
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types?.Where(t => t != null);
            }
        }
    }
}