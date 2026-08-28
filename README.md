# Vehicle Explorer

Pick a car make and a model year, and see the vehicle types and models available for
that combination. Data comes from the NHTSA vPIC catalogue.

**Live:** <https://de85sfq3gjap3.cloudfront.net>

> The hosted instance runs on a personal AWS account and may be torn down after review.
> If it is unreachable, `docker compose up --build` gives you the same application in a
> couple of minutes.

---

## Contents

- [Running it](#running-it)
- [Tests](#tests)
- [The API](#the-api)
- [Architecture](#architecture)
- [Behaviour worth knowing about](#behaviour-worth-knowing-about)
- [Deployment](#deployment)
- [Trade-offs](#trade-offs)

---

## Running it

### With Docker — one command

The only prerequisite is Docker.

```bash
git clone https://github.com/taherelhares/vehicle-explorer.git
cd vehicle-explorer
docker compose up --build
```

Then open **http://localhost:8080**.

The first build takes several minutes: it pulls the Node, .NET SDK and ASP.NET runtime
images and runs both `npm ci` and `dotnet restore` from cold. Later builds reuse those
layers.

One port serves everything. The API answers `/api/*` and `/health`; every other path is
served from the React build, so `http://localhost:8080/api/vehicles/makes` returns JSON
and everything else returns the application.

Stop with `Ctrl+C`, then `docker compose down`.

### Without Docker — for development

Two processes, because the Vite dev server provides hot reload that a built bundle
cannot.

**Prerequisites:** .NET 10 SDK, Node 20 or newer.

Terminal one — the API:

```bash
cd src/VehicleExplorer.Api
dotnet run --launch-profile https
```

Serves on `https://localhost:7079` and `http://localhost:5020`.

Terminal two — the client:

```bash
cd client
npm install
npm run dev
```

Open **http://localhost:5173**.

In this mode the two halves are separate origins, so `client/.env.development` points the
client at the API and the API allows `http://localhost:5173` through CORS. Neither is
needed in the container, where there is only one origin.

---

## Tests

```bash
dotnet test          # .NET, from the repository root
cd client && npm test   # client
```

Four .NET test projects, split by what they actually exercise:

| Project | Covers |
|---|---|
| `VehicleExplorer.Application.Tests` | Caching policy and failure paths, against a stubbed port |
| `VehicleExplorer.Infrastructure.Tests` | vPIC response mapping and failure translation |
| `VehicleExplorer.Infrastructure.IntegrationTests` | Transport behaviour against a stub message handler |
| `VehicleExplorer.Api.IntegrationTests` | Endpoint routing, status codes and CORS |

On the client, Vitest and Testing Library cover the service layer, the data-loading hook
and the top-level component.

---

## The API

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/vehicles/makes` | Every make in the catalogue |
| `GET` | `/api/vehicles/makes/{makeId}/vehicle-types` | Vehicle types recorded for a make |
| `GET` | `/api/vehicles/makes/{makeId}/models?year={year}&vehicleType={type}` | Models for a make and year, optionally narrowed by type |
| `GET` | `/health` | Liveness, for the deployment script and health probes |

All three return the same shape — `{ "id": number, "name": string }` — regardless of the
inconsistent casing vPIC uses across its own endpoints.

Upstream data source:

| Purpose | vPIC endpoint |
|---|---|
| All makes | `getallmakes` |
| Types for a make | `GetVehicleTypesForMakeId/{makeId}` |
| Models for make and year | `GetModelsForMakeIdYear/makeId/{makeId}/modelyear/{year}` |

---

## Architecture

Three projects, ports and adapters:

```
VehicleExplorer.Api  ──►  VehicleExplorer.Application  ◄──  VehicleExplorer.Infrastructure
```

`Application` references nothing. `Infrastructure` references `Application`. `Api`
references both, but only in `Program.cs` to wire up dependency injection — the standard
composition-root exception.

**The justification is not domain complexity — there is none.** This is a thin read model
over a third-party API that is slow and intermittently unavailable. The architecture
exists to isolate that API behind a port, so every quirk of it lives in one adapter and
nowhere else.

### Two interfaces, deliberately

- **`INhtsaClient`** lives in `Application` and speaks this application's language:
  `Task<IReadOnlyList<MakeDto>> GetMakesAsync(CancellationToken)`.
- **`INhtsaApi`** lives in `Infrastructure`, is the Refit interface, and speaks vPIC's:
  envelopes, `Make_ID`, `format=json` on every route. It is `internal`.
- **`NhtsaClient`** implements the first by calling the second, maps between the two
  vocabularies, and translates transport failures into `NhtsaUnavailableException`.

Injecting `INhtsaApi` straight into the service would be one file simpler, but it would
drag `NhtsaResponse<NhtsaMake>` and a Refit dependency into `Application`. `NhtsaClient`
is the seam that prevents that.

The rule that keeps it honest: **grep the repository for `Make_ID`.** It appears only
under `Infrastructure`. Nothing in `Application` or `Api` knows that name exists.

### One image

The React build is not a separate service, because it is not a server — it is a folder of
static files, and the API can serve a folder. The Dockerfile has three stages: Node builds
the client, the .NET SDK publishes the API, and the runtime image receives both with the
client output in `wwwroot`.

That gives one container, one port, one origin. Same-origin also means production needs no
CORS at all, and the client calls `/api/...` with no host in front of it.

---

## Behaviour worth knowing about

**The makes list is roughly 14,000 rows.** It is cached server-side and presented through
a searchable autocomplete. A native `<select>` with 14,000 options is where this demo
would otherwise fall over.

**Caching.** Results are held in memory for 24 hours. The catalogue changes when a
manufacturer registers a make or files a model year — a matter of months — so a day is
well inside the useful window. Empty results are deliberately *not* cached: vPIC
occasionally returns a well-formed envelope with no rows, and caching that would turn a
momentary glitch into a day of an empty dropdown.

**Resilience.** vPIC is slow and returns 5xx intermittently, so calls go through
`AddStandardResilienceHandler`: retries, a circuit breaker, a 20-second budget per attempt
and 60 seconds for the whole call including retries. `HttpClient`'s own 100-second default
is disabled so it cannot silently cap that budget.

**Errors.** An unreachable upstream becomes a `503` with an RFC 7807 problem details body.
The client distinguishes that one case from every other failure, because "the data
provider is down, try again" is the only distinction the interface actually acts on.

**The vehicle type filter.** The brief asks for models by year *and* vehicle type, but the
URL it gives filters only by make and year. vPIC does accept `vehicleType` as a query
parameter and honours it — make 474, year 2015 returns 62 models unfiltered and 1 with
`vehicleType=truck` — so the filter is pushed upstream rather than applied to a larger
result set locally. A blank value is normalised to null so the parameter is omitted
entirely.

**Unknown ids.** vPIC answers an unrecognised make with a successful, empty envelope
rather than a 404, so an unknown id is indistinguishable from a make with no recorded
types. Both are reported as an empty list.

---

## Deployment

### What runs where

```
GitHub Actions ──(OIDC)──► AWS
      │
      ├─► ECR                    image, tagged with the commit SHA
      │
      └─► SSM Send-Command ─────► EC2 t3.micro
                                      │  docker run -p 80:8080
                                      ▼
                                  CloudFront ──► https://<id>.cloudfront.net
```

Every push to `master` builds the image, pushes it to ECR tagged with the commit SHA,
tells the instance to pull and restart over SSM, then polls `/health` through the public
URL and fails the run if it never returns 200.

Choices worth naming:

- **CloudFront is there for the certificate.** It provides HTTPS on `*.cloudfront.net`
  with no domain to buy and no certificate to renew. It also caches the fingerprinted
  assets while leaving `/api/*` and `/index.html` uncached — the latter because a cached
  shell would keep pointing at the previous deploy's bundles.
- **No AWS access keys exist.** GitHub presents a short-lived OIDC token scoped to one
  branch of one repository and receives temporary credentials.
- **No SSH, no key pair, no port 22.** The rollout goes over SSM Send-Command.
- **Port 80 accepts only CloudFront**, via the `com.amazonaws.global.cloudfront.origin-facing`
  managed prefix list. Without that, the instance's own address would serve the
  application over plain HTTP and bypass TLS entirely.
- **Images are tagged with the commit SHA, not `latest`**, so what is deployed is always
  traceable to a commit and a rollback is a redeploy of an earlier tag rather than a
  rebuild.

Everything above is defined in `infra/vehicle-explorer.yml`. There are no manual console
steps.

### Reproducing it

**Prerequisites:** an AWS account and the AWS CLI. AWS CloudShell has the CLI already
authenticated, which avoids creating access keys.

**1. Find the CloudFront prefix list for your region.** The id differs per region.

```bash
aws ec2 describe-managed-prefix-lists --region eu-central-1 \
  --filters Name=prefix-list-name,Values=com.amazonaws.global.cloudfront.origin-facing \
  --query "PrefixLists[0].PrefixListId" --output text
```

**2. Find your repository's numeric ids.** Repositories created on or after 15 July 2026
present an OIDC subject that embeds them.

```bash
curl -s https://api.github.com/repos/<owner>/<repo> | jq '{repo_id: .id, owner_id: .owner.id}'
```

**3. Deploy the stack.** Around 15 minutes; the CloudFront distribution is most of it.

```bash
aws cloudformation deploy \
  --region eu-central-1 \
  --stack-name vehicle-explorer \
  --template-file infra/vehicle-explorer.yml \
  --capabilities CAPABILITY_NAMED_IAM \
  --parameter-overrides CloudFrontPrefixListId=pl-xxxxxxxx \
                        GitHubRepo=<owner>/<repo> \
                        GitHubBranch=master \
                        GitHubOwnerId=<owner_id> \
                        GitHubRepositoryId=<repo_id>
```

**4. Read the outputs.**

```bash
aws cloudformation describe-stacks --region eu-central-1 \
  --stack-name vehicle-explorer --query "Stacks[0].Outputs" --output table
```

**5. Set them in GitHub**, under Settings → Secrets and variables → Actions:

| Store | Name | From output |
|---|---|---|
| Secret | `AWS_DEPLOY_ROLE_ARN` | `DeployRoleArn` |
| Secret | `EC2_INSTANCE_ID` | `InstanceId` |
| Variable | `SITE_URL` | `SiteUrl` |

`SITE_URL` is a variable rather than a secret on purpose: GitHub masks secrets in logs,
which would render the health check's output unreadable. It is a public URL.

**6. Push to `master`.** The registry is empty until the first run, which is why the stack
installs Docker but starts no container.

**Teardown**, so nothing keeps billing:

```bash
aws cloudformation delete-stack --region eu-central-1 --stack-name vehicle-explorer
```

ECR refuses to delete a repository that still holds images — empty it first, then re-run.

---

## Trade-offs

### Deliberately left out

Each of these would be reasonable in a larger system and is dead weight in three read
endpoints over public data:

- **MediatR** — one call site per endpoint. A mediator would add indirection between the
  endpoint and the one service it calls, and nothing would be decoupled by it.
- **AutoMapper** — the mappings are three small projections in one adapter. Convention-based
  mapping would hide exactly the vPIC-to-domain translation that is the point of that class.
- **Repository interfaces** — there is no persistence to abstract. `INhtsaClient` is
  already the port; a repository over it would be a second name for the same seam.
- **A Domain project** — there is no domain logic. Makes, types and models are data passing
  through. An anaemic project of DTOs would be structure without substance.
- **Persistence** — nothing needs to survive a restart. The cache is a latency optimisation,
  not a store.
- **Authentication** — public, read-only data with no per-user state.

Adding any of them would read as an inability to size a solution to its problem.

### Known limitations

Honest about what this does not do:

- **Redeploys drop requests for a second or two**, between `docker rm -f` and the new
  container accepting connections. Zero-downtime would mean two containers and a reverse
  proxy — more machinery than a demonstration justifies, but it is a real gap.
- **One instance, no autoscaling and no redundancy.** If it fails, the site is down until
  it comes back. Correct for free-tier hosting; not something to ship to production.
- **The cache is per-process and dies with the container**, so the first request after each
  deploy pays full upstream latency. A shared cache would fix it and would also introduce a
  service to run and pay for.
- **`/health` reports only that the container is serving.** It does not check vPIC, on
  purpose: reporting someone else's outage as our own would make the deployment gate lie.
- **No end-to-end browser tests.** The unit and integration layers cover the logic; the
  assembled page in a real browser is verified by hand.
- **CloudFront caching means a deploy is not instantly visible everywhere.** `/index.html`
  is uncached to keep the shell current, but edge propagation is not instantaneous.
