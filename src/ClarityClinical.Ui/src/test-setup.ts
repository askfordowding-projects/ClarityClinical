import { expect, vi } from 'vitest';

type CompatSpy = ReturnType<typeof vi.fn> & {
  and: {
    returnValue(value: unknown): CompatSpy;
    resolveTo(value: unknown): CompatSpy;
    rejectWith(reason: unknown): CompatSpy;
    callFake(fn: (...args: any[]) => any): CompatSpy;
    returnValues(...values: unknown[]): CompatSpy;
  };
  calls: {
    reset(): void;
    mostRecent(): { args: unknown[] };
    argsFor(index: number): unknown[];
    count(): number;
  };
};

function decorateSpy(spy: ReturnType<typeof vi.fn>): CompatSpy {
  const compat = spy as CompatSpy;

  compat.and = {
    returnValue(value) {
      compat.mockReturnValue(value);
      return compat;
    },
    resolveTo(value?) {
      compat.mockResolvedValue(value);
      return compat;
    },
    rejectWith(reason) {
      compat.mockRejectedValue(reason);
      return compat;
    },
    callFake(fn) {
      compat.mockImplementation(fn);
      return compat;
    },
    returnValues(...values) {
      let index = 0;
      compat.mockImplementation(() => values[index++]);
      return compat;
    },
  };

  compat.calls = {
    reset() {
      compat.mockClear();
    },
    mostRecent() {
      const calls = compat.mock.calls;
      return { args: calls.length ? calls[calls.length - 1] : [] };
    },
    argsFor(index) {
      return compat.mock.calls[index] ?? [];
    },
    count() {
      return compat.mock.calls.length;
    },
  };

  return compat;
}

function createSpy(name?: string): CompatSpy {
  const spy = decorateSpy(vi.fn());
  if (name) {
    spy.mockName(name);
  }
  return spy;
}

function createSpyObj<T>(baseName: string, methodNames: readonly string[] | Record<string, unknown>): T {
  const target: Record<string, unknown> = {};
  const entries = Array.isArray(methodNames)
    ? methodNames.map((name) => [name, undefined] as const)
    : Object.entries(methodNames);

  for (const [methodName, returnValue] of entries) {
    const spy = createSpy(`${baseName}.${methodName}`);
    if (!Array.isArray(methodNames)) {
      spy.and.returnValue(returnValue);
    }
    target[methodName] = spy;
  }

  return target as T;
}

expect.extend({
  toBeTrue(received: unknown) {
    const pass = received === true;
    return { pass, message: () => `expected ${String(received)} ${pass ? 'not ' : ''}to be true` };
  },
  toBeFalse(received: unknown) {
    const pass = received === false;
    return { pass, message: () => `expected ${String(received)} ${pass ? 'not ' : ''}to be false` };
  },
  toHaveSize(received: unknown, expectedSize: number) {
    const actual = received != null && typeof received === 'object' && 'size' in received
      ? (received as { size: number }).size
      : received != null && typeof received === 'object' && 'length' in received
        ? (received as { length: number }).length
        : undefined;
    const pass = actual === expectedSize;
    return { pass, message: () => `expected size ${String(actual)} ${pass ? 'not ' : ''}to equal ${expectedSize}` };
  },
  toHaveBeenCalledOnceWith(received: unknown, ...expectedArgs: unknown[]) {
    const calls = (received as { mock?: { calls?: unknown[][] } })?.mock?.calls ?? [];
    const pass = calls.length === 1 && this.equals(calls[0], expectedArgs);
    return {
      pass,
      message: () => `expected spy ${pass ? 'not ' : ''}to have been called exactly once with the expected arguments`,
    };
  },
});

Object.defineProperty(URL, 'createObjectURL', {
  configurable: true,
  writable: true,
  value: (_blob: Blob) => 'blob:gastrohub-test',
});

Object.defineProperty(URL, 'revokeObjectURL', {
  configurable: true,
  writable: true,
  value: (_url: string) => undefined,
});
if (typeof window.matchMedia !== 'function') {
  Object.defineProperty(window, 'matchMedia', {
    configurable: true,
    writable: true,
    value: (query: string): MediaQueryList => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: () => undefined,
      removeListener: () => undefined,
      addEventListener: () => undefined,
      removeEventListener: () => undefined,
      dispatchEvent: () => true,
    }),
  });
}
const jasmineCompat = {
  createSpy,
  createSpyObj,
  objectContaining: expect.objectContaining,
  any: expect.any,
};

Object.assign(globalThis, {
  jasmine: jasmineCompat,
  spyOn<T extends object, K extends keyof T>(object: T, method: K) {
    return decorateSpy(vi.spyOn(object, method as never) as unknown as ReturnType<typeof vi.fn>);
  },
  expectAsync<T>(promise: Promise<T>) {
    return {
      async toBeRejected() {
        let rejected = false;
        try {
          await promise;
        } catch {
          rejected = true;
        }
        if (!rejected) {
          throw new Error('Expected promise to be rejected.');
        }
      },
    };
  },
});
