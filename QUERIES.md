# Anar

# Influx DB Queries

The queries below can be used in the InfluxDB Gui or Grafana for building
dashboards.

## Current Output (Now)
This used to be available via `/api/v1/production`. It stopped working on my system after a firmware upgrade. You may have better luck.

```javascript
from(bucket: "solar")
  |> range(start: -1h)
  |> filter(fn: (r) => r["_measurement"] == "totals")
  |> filter(fn: (r) => r["_field"] == "wattsNow")
  |> last()
```

## Today
Shows accumulated energy generation from midnight of current day.
This used to be available via `/api/v1/production`.

```javascript
// Calculations based on pushing data at 5m intervals.
// If that is changed, this query also needs to be modified.
from(bucket: "solar")
  |> range(start: today())
  |> filter(fn: (r) => r["_measurement"] == "totals")
  |> filter(fn: (r) => r["_field"] == "wattsNow")
  |> aggregateWindow(every: 5m, fn: sum)
  |> map(fn: (r) => ({
      _time: r._time,
      // Convert to kWh
      _value: float(v: r._value) * 5.0 / 60.0 / 1000.0
  }))
  |> cumulativeSum(columns: ["_value"])
  |> last()
```

## This Week
Shows accumulated energy generation from Monday of current week.
This used to be available via `/api/v1/production`.

```javascript
// Calculations based on pushing data at 5m intervals.
// If that is changed, this query also needs to be modified.
import "experimental"
import "date"
from(bucket: "solar")
  // UNIX starts the week on Thursday. Shift it to Monday.
  |> range(start: experimental.subDuration(d: 3d, from: date.truncate(t: now(), unit: 1w)))
  |> filter(fn: (r) => r["_measurement"] == "totals")
  |> filter(fn: (r) => r["_field"] == "wattsNow")
  |> aggregateWindow(every: 5m, fn: mean, createEmpty: false)
  |> map(fn: (r) => ({
      // Convert to kWh
    _time: r._time,
    _value: float(v: r._value) * 5.0 / 60.0 / 1000.0
  }))
  |> cumulativeSum(columns: ["_value"])
  |> last()
```

### This month
Shows accumulated energy generation from first day of current month.

```javascript
import "date"
// Calculations based on pushing data at 5m intervals.
// If that is changed, this query also needs to be modified.
from(bucket: "solar")
  |> range(start: date.truncate(t: now(), unit: 1mo))
  |> filter(fn: (r) => r["_measurement"] == "totals")
  |> filter(fn: (r) => r["_field"] == "wattsNow")
  |> aggregateWindow(every: 5m, fn: sum)
  |> map(fn: (r) => ({
      _time: r._time,
      // Convert integer watts to kWh
      _value: float(v: r._value) * 5.0 / 60.0 / 1000.0
  }))
  |> cumulativeSum(columns: ["_value"])
  |> last()
```

### Inverters
Shows current output by inverter.

```javascript
import "strings"
from(bucket: "solar")
  |> range(start: -1h)
  |> filter(fn: (r) => r["_measurement"] == "inverter")
  |> filter(fn: (r) => r["_field"] == "watts")
  |> sort(columns: ["arrayName", "serialNumber"])
  |> map(fn: (r) => ({
    // only keep enough to uniquely identify inverter.
    r with serialNumber: strings.substring(v: r.serialNumber, start: 7, end: 12)
  }))
  |> last()
```

### Arrays
Shows current total by array and facing.

```javascript
from(bucket: "solar")
  |> range(start: -1h)
  |> filter(fn: (r) => r["_measurement"] == "inverter")
  |> filter(fn: (r) => r["_field"] == "watts")
  |> last()
  |> group(columns: ["arrayName", "facing"])
  |> sum()
```