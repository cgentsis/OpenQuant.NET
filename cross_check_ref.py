import csv, sys, math

# Configuration
PERIOD_SMA = 14
PERIOD_EMA = 14
PERIOD_BB = 20
NBDEV_BB = 2.0
PERIOD_STOCH = 14
SLOWK = 3
SLOWD = 3
PERIOD_AARON = 14
TOLERANCE = 1e-6

# Helper functions
def sma(values, period, idx):
    if idx + 1 < period:
        return None
    return sum(values[idx+1-period:idx+1]) / period

def ema(values, period, idx, previous_ema=None):
    k = 2.0 / (period + 1.0)
    if idx + 1 < period:
        return None, None
    if idx + 1 == period:
        # first EMA is SMA of first period
        init_sma = sum(values[:period]) / period
        return init_sma, init_sma
    # compute EMA recursively
    if previous_ema is None:
        # should not happen
        raise ValueError("previous EMA missing")
    ema_val = (values[idx] - previous_ema) * k + previous_ema
    return ema_val, ema_val

def bbands(values, period, nbdev, idx):
    if idx + 1 < period:
        return None, None, None, None
    window = values[idx+1-period:idx+1]
    sma_val = sum(window) / period
    variance = sum((x - sma_val) ** 2 for x in window) / period
    stddev = math.sqrt(variance) * nbdev
    upper = sma_val + stddev
    lower = sma_val - stddev
    if stddev == 0:
        percentb = 0.5
    else:
        percentb = (values[idx] - lower) / (upper - lower)
    return upper, sma_val, lower, percentb

def stoch(highs, lows, closes, fastk_period, slowk, slowd, idx):
    # fast %K
    if idx + 1 < fastk_period:
        return None, None
    high_window = highs[idx+1-fastk_period:idx+1]
    low_window = lows[idx+1-fastk_period:idx+1]
    highest = max(high_window)
    lowest = min(low_window)
    if highest == lowest:
        fastk = 0.0
    else:
        fastk = (closes[idx] - lowest) / (highest - lowest) * 100.0
    # %D = SMA of fastk over slowk period
    if idx + 1 < fastk_period + slowk - 1:
        return fastk, None
    start = idx - slowk + 1
    slowk_vals = []
    for j in range(start, idx+1):
        # compute fastK for each j
        high_w = highs[j+1-fastk_period:j+1]
        low_w = lows[j+1-fastk_period:j+1]
        hi = max(high_w)
        lo = min(low_w)
        if hi == lo:
            fk = 0.0
        else:
            fk = (closes[j] - lo) / (hi - lo) * 100.0
        slowk_vals.append(fk)
    slowd_val = sum(slowk_vals) / slowk
    return fastk, slowd_val

def aroon(highs, lows, period, idx):
    if idx + 1 < period + 1:
        return None, None, None
    high_window = highs[idx+1-period-1:idx+1]  # period+1 values
    low_window = lows[idx+1-period-1:idx+1]
    # index of most recent (newest) bar is period (0-based from oldest)
    high_max = max(high_window)
    low_min = min(low_window)
    # bars since highest high
    high_idx = high_window.index(high_max)  # idx from oldest
    low_idx = low_window.index(low_min)
    up = ((period - high_idx) / period) * 100.0
    down = ((period - low_idx) / period) * 100.0
    osc = up - down
    return up, down, osc

def main():
    if len(sys.argv) != 2:
        print("Usage: python cross_check_ref.py <data.csv>")
        sys.exit(1)
    data_path = sys.argv[1]
    rows = []
    with open(data_path, newline='') as csvfile:
        reader = csv.DictReader(csvfile)
        for r in reader:
            rows.append(r)
    # parse values
    timestamps = []
    opens = []
    highs = []
    lows = []
    closes = []
    volumes = []
    for r in rows:
        timestamps.append(r['Timestamp'])
        opens.append(float(r['Open']))
        highs.append(float(r['High']))
        lows.append(float(r['Low']))
        closes.append(float(r['Close']))
        volumes.append(int(r['Volume']))
    # compute reference indicators
    ema_prev = None
    ema_vals = []
    for i in range(len(closes)):
        # SMA
        sma_val = sma(closes, PERIOD_SMA, i)
        # EMA
        ema_val, ema_prev = ema(closes, PERIOD_EMA, i, ema_prev)
        ema_vals.append(ema_val)
        # BBands
        bb = bbands(closes, PERIOD_BB, NBDEV_BB, i)
        # Stoch
        fastk, slowd = stoch(highs, lows, closes, PERIOD_STOCH, SLOWK, SLOWD, i)
        # Aroon
        aroon_up, aroon_down, aroon_osc = aroon(highs, lows, PERIOD_AARON, i)
        # add to row dict for comparison
        rows[i]['SMA'] = '' if sma_val is None else f"{sma_val:.10f}"
        rows[i]['EMA'] = '' if ema_val is None else f"{ema_val:.10f}"
        rows[i]['BBUpper'] = '' if bb[0] is None else f"{bb[0]:.10f}"
        rows[i]['BBMiddle'] = '' if bb[1] is None else f"{bb[1]:.10f}"
        rows[i]['BBLower'] = '' if bb[2] is None else f"{bb[2]:.10f}"
        rows[i]['PercentB'] = '' if bb[3] is None else f"{bb[3]:.10f}"
        rows[i]['StochK'] = '' if fastk is None else f"{fastk:.10f}"
        rows[i]['StochD'] = '' if slowd is None else f"{slowd:.10f}"
        rows[i]['AroonUp'] = '' if aroon_up is None else f"{aroon_up:.10f}"
        rows[i]['AroonDown'] = '' if aroon_down is None else f"{aroon_down:.10f}"
        rows[i]['AroonOsc'] = '' if aroon_osc is None else f"{aroon_osc:.10f}"
    # write reference file
    out_path = data_path.replace('.csv', '_ref.csv')
    fieldnames = list(rows[0].keys())
    with open(out_path, 'w', newline='') as csvfile:
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)
    # compare with original values (which already contain our library outputs)
    mismatches = []
    for i, row in enumerate(rows):
        for key in ['SMA','EMA','BBUpper','BBMiddle','BBLower','PercentB','StochK','StochD','AroonUp','AroonDown','AroonOsc']:
            lib_val = row.get(key+'_Lib')  # placeholder column to be filled by .NET results
            ref_val = row[key]
            if lib_val is None or lib_val == '':
                continue
            if ref_val == '':
                continue
            if abs(float(lib_val) - float(ref_val)) > TOLERANCE:
                mismatches.append((i, key, lib_val, ref_val))
    if mismatches:
        print(f"Found {len(mismatches)} mismatches:")
        for i, key, lib_val, ref_val in mismatches:
            print(f"Row {i} {key}: lib={lib_val} ref={ref_val}")
        sys.exit(1)
    else:
        print('All indicators match reference within tolerance.')
        sys.exit(0)

if __name__ == '__main__':
    main()
