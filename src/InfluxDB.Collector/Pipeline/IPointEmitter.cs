namespace InfluxDB.Collector.Pipeline
{
    interface IPointEmitter
    {
        void Emit(PointData[] points);

        void Emit(PointData point);
    }
}
