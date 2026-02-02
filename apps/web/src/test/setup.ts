import "@testing-library/jest-dom/vitest";
import { afterAll, afterEach, beforeAll } from "vitest";
import { server } from "./server";

// Start MSW before all tests
beforeAll(() => server.listen({ onUnhandledRequest: "error" }));

// Reset handlers after each test (so tests don’t leak state)
afterEach(() => server.resetHandlers());

// Clean up
afterAll(() => server.close());
