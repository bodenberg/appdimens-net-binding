# Release Runbook

## Pre-release

1. Bump `<Version>` in `Directory.Build.props`
2. Update `CHANGELOG.md` and README install examples
3. Run `python3 scripts/generate-dimens.py`
4. Run `./scripts/publish-nuget.sh` (pack + verify, no push)
5. Confirm CI green on `main`

## Publish

```bash
export NUGET_API_KEY='...'
./scripts/publish-nuget.sh --push
```

## Rollback

1. **NuGet:** unlist broken version on nuget.org (cannot delete)
2. **Git:** revert commit, tag `vX.Y.Z+1` hotfix
3. **Consumers:** pin previous package version in csproj

## Breaking changes (layout v2)

- `Generated/{sw,w,h}/` removed; use flat `Dimens.{N}.xaml`
- `buckets.json` includes `layoutVersion: 2`
- Loader supports legacy layout one release via `sw/w/h` folders if present
