import type { MockInstance } from 'vitest';

declare module 'vitest' {
  interface Assertion<T = any> {
    toBeTrue(): void;
    toBeFalse(): void;
    toHaveSize(expected: number): void;
    toHaveBeenCalledOnceWith(...args: any[]): void;
  }
}

declare global {
  namespace jasmine {
    interface SpyAnd {
      returnValue(value: unknown): Spy;
      resolveTo(value?: unknown): Spy;
      rejectWith(reason: unknown): Spy;
      callFake(fn: (...args: any[]) => any): Spy;
      returnValues(...values: unknown[]): Spy;
    }

    interface Calls {
      reset(): void;
      mostRecent(): { args: any[] };
      argsFor(index: number): any[];
      count(): number;
    }

    interface Spy extends MockInstance {
      (...args: any[]): any;
      and: SpyAnd;
      calls: Calls;
    }

    type SpyObj<T> = T & {
      [K in keyof T]: T[K] extends (...args: any[]) => any ? T[K] & Spy : T[K];
    };

    function createSpyObj(baseName: string, methodNames: readonly string[] | Record<string, unknown>): any;
    function createSpyObj<T>(baseName: string, methodNames: readonly string[] | Record<string, unknown>): SpyObj<T>;
    function createSpy(name?: string): Spy;
    function objectContaining<T>(sample: T): any;
    function any(expected: unknown): any;
  }

  function spyOn<T extends object, K extends keyof T>(object: T, method: K): jasmine.Spy;
  function expectAsync<T>(promise: Promise<T>): { toBeRejected(): Promise<void> };
}

export {};
