# Legal AI Integration

This document describes how the Minicato C# platform integrates with the Python Legal RAG engine.

## Current Staging Setup

```text
Flutter
-> Minicato.Api
-> Hugging Face Space
-> Python Legal RAG /chat
-> Gemini primary + Groq fallback
```

Flutter must call the C# backend only:

```http
POST /api/legal-ai/chat
```

Optional Flutter metadata endpoint:

```http
GET /api/legal-ai/info
```

Request:

```json
{
  "query": "ما ضمانات الحرية الشخصية في الدستور المصري؟"
}
```

The C# backend is responsible for authentication, user identity, roles, logging, future quota checks, and hiding internal AI runtime details from clients.

Flutter must never call the Hugging Face Space directly in production.

## Python Runtime

The current Python runtime is hosted on Hugging Face:

```text
https://loaywael10-al-mostashar-legal-rag.hf.space
```

Large runtime assets are not stored in the Space repository. They are stored in a separate Hugging Face Dataset repository and downloaded lazily by the Python service:

```text
loaywael10/al-mostashar-legal-rag-assets
```

## C# Environment Variables

Configure the C# platform with:

```env
LegalAi__BaseUrl=https://loaywael10-al-mostashar-legal-rag.hf.space
LegalAi__ApiKey=<internal token when Python protection is enabled>
LegalAi__HeaderName=X-Internal-Service-Token
LegalAi__TimeoutSeconds=120
LegalAi__WarmupOnStartup=false
LegalAi__WarmupDelaySeconds=5
LegalAi__CacheEnabled=true
LegalAi__CacheDurationMinutes=30
```

Do not store the internal token in `appsettings.json`. Use environment variables, user secrets, or deployment secrets.

The C# gateway can optionally warm the AI worker shortly after startup. Warmup failures are logged and do not crash the API. The gateway can also cache successful non-user-specific Legal AI responses for repeated identical normalized queries. Disable or scope this cache by user if future personalization is added.

For future internal Docker deployment, `LegalAi__BaseUrl` can become:

```env
LegalAi__BaseUrl=http://legal-rag-api:7860
```

## Python / Hugging Face Environment Variables

When C# integration is ready to be the only public caller, protect the Python endpoints:

```env
REQUIRE_INTERNAL_API_TOKEN=true
INTERNAL_API_TOKEN=<same secret as LegalAi__ApiKey>
INTERNAL_API_TOKEN_HEADER=X-Internal-Service-Token
ENABLE_PUBLIC_DOCS=false
PROTECT_LEGAL_INFO=false
```

Existing LLM variables:

```env
GEMINI_API_KEY=<gemini key>
GROQ_API_KEY=<groq key>
LLM_PROVIDER_NAME=gemini
LLM_FALLBACK_PROVIDER_NAME=groq
GROQ_MODEL=llama-3.3-70b-versatile
```

If the Hugging Face assets dataset is private:

```env
HF_TOKEN=<token with read access>
```

## Endpoint Protection

## C# Gateway Endpoints

Flutter/client-facing:

- `POST /api/legal-ai/chat`: sends a user legal question through the authenticated C# backend.
- `GET /api/legal-ai/info`: returns sanitized AI worker metadata if the app needs it.

Admin/system:

- `POST /api/legal-ai/warmup`: warms the Python worker and is restricted to `Admin`.
- `GET /api/legal-ai/health`: authenticated diagnostic endpoint for worker health.

Flutter must not call these Hugging Face endpoints directly:

- `/chat`
- `/warmup`
- `/docs`
- `/legal-answer`
- `/embed`
- `/embed/batch`

## Flutter Response Rendering

The chat response keeps `final_answer` for backward compatibility.

When `answer_parts` is present, Flutter should prefer it for mobile UI:

- `intro`
- `section_title`
- `bullets`
- `legal_basis`
- `note`
- source cards from `sources`

If `answer_parts` is `null`, Flutter should fall back to rendering `final_answer`.

Successful chat responses may include diagnostic response headers from the C# gateway:

- `X-Legal-AI-Cache`: `HIT` or `MISS`.
- `X-Legal-AI-Elapsed-Ms`: total gateway processing time in milliseconds.

These headers do not expose the Hugging Face URL, internal token, or provider error details.

## Python Endpoint Protection

When `REQUIRE_INTERNAL_API_TOKEN=true`, Python protects:

- `POST /chat`
- `POST /warmup`
- `POST /legal-answer`
- `POST /embed`
- `POST /embed/batch`
- `GET /info`

Python keeps these endpoints public:

- `GET /health`
- `GET /legal-info` if `PROTECT_LEGAL_INFO=false`

When `ENABLE_PUBLIC_DOCS=false`, these endpoints return 404:

- `GET /docs`
- `GET /redoc`
- `GET /openapi.json`

## Production Target

The ideal production topology is:

```text
Flutter
-> Minicato.Api
-> internal Python Legal RAG service on the same Docker/network
```

In that topology, the Python service should not publish its port publicly. The C# API remains the official public backend.

## Rate Limits and Quotas

AI usage limits should live in the C# platform because C# knows the authenticated user, role, and future subscription state.

Recommended future policies:

- Per-minute authenticated user limit.
- Daily AI question quota for free users.
- Role/subscription-based quota overrides.
- Admin override.
- Optional short cache for identical normalized questions when the response is not user-specific.

This integration does not persist chat history yet. Add persistence later through an application feature that records user id, query, response mode, source count, and latency.
