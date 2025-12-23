using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WodToolKit.Cache
{
    /// <summary>
    /// 基于内存的临时缓存实现（非持久化）
    /// </summary>
    public sealed class TempCache<TKey, TValue> : IDisposable
        where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, CacheItem> _cache = new ConcurrentDictionary<TKey, CacheItem>();

        // 明确指定使用 System.Threading.Timer
        private readonly System.Threading.Timer _cleanupTimer;

        private readonly TimeSpan _cleanupInterval;
        private readonly int _defaultTtlSeconds;

        public TempCache(TimeSpan cleanupInterval, int defaultTtlSeconds = 300)
        {
            if (cleanupInterval <= TimeSpan.Zero)
                throw new ArgumentException("清理间隔必须大于0", nameof(cleanupInterval));
            if (defaultTtlSeconds <= 0)
                throw new ArgumentException("默认缓存时间必须大于0", nameof(defaultTtlSeconds));

            _cleanupInterval = cleanupInterval;
            _defaultTtlSeconds = defaultTtlSeconds;

            // 使用完整命名空间初始化Timer
            _cleanupTimer = new System.Threading.Timer(
                callback: RemoveExpiredItems,
                state: null,
                dueTime: _cleanupInterval,
                period: _cleanupInterval
            );
        }

        /// <summary>
        /// 添加或更新缓存项
        /// </summary>
        public void Set(TKey key, TValue value, int? ttlSeconds = null)
        {
            var expiration = DateTimeOffset.UtcNow.AddSeconds(ttlSeconds ?? _defaultTtlSeconds);
            _cache[key] = new CacheItem(value, expiration);
        }

        /// <summary>
        /// 尝试获取缓存值（同时检查是否过期）
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                if (item.Expiration > DateTimeOffset.UtcNow)
                {
                    value = item.Value;
                    return true;
                }

                // 惰性清理：发现过期立即移除
                _cache.TryRemove(key, out _);
            }

            value = default;
            return false;
        }

        /// <summary>
        /// 移除指定缓存项
        /// </summary>
        public bool Remove(TKey key) => _cache.TryRemove(key, out _);

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public void Clear() => _cache.Clear();

        /// <summary>
        /// 获取当前缓存项数量
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// 统计已过期的缓存项数量
        /// </summary>
        public int CountExpiredItems()
        {
            var now = DateTimeOffset.UtcNow;
            int expiredCount = 0;

            foreach (var kvp in _cache)
            {
                if (kvp.Value.Expiration <= now)
                {
                    expiredCount++;
                }
            }
            return expiredCount;
        }

        /// <summary>
        /// 释放资源（停止清理定时器）
        /// </summary>
        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }

        /// <summary>
        /// 定期清理过期项
        /// </summary>
        private void RemoveExpiredItems(object state) // 添加object参数以匹配TimerCallback签名
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var kvp in _cache)
            {
                if (kvp.Value.Expiration <= now)
                {
                    _cache.TryRemove(kvp.Key, out _);
                }
            }
        }

        private sealed class CacheItem
        {
            public TValue Value { get; }
            public DateTimeOffset Expiration { get; }

            public CacheItem(TValue value, DateTimeOffset expiration)
            {
                Value = value;
                Expiration = expiration;
            }
        }
    }
}
