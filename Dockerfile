# ====== Production ====== #
FROM mcr.microsoft.com/dotnet/runtime:7.0-bookworm-slim-amd64 as final
WORKDIR /app

# ====== Build image ====== #
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS publish
ARG RELEASE_VERSION
WORKDIR /sln

COPY ./*.sln ./

# Copy the main source project files
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done

# Copy the test project files
#COPY test/*/*.csproj ./
#RUN for file in $(ls *.csproj); do mkdir -p test/${file%.*}/ && mv $file test/${file%.*}/; done

RUN dotnet restore

#COPY ./test ./test
COPY ./src ./src
RUN dotnet build -c Release --no-restore -o /app/build -p:VersionPrefix=$RELEASE_VERSION

RUN dotnet publish "./src/Authn/Authn.csproj" -c Release -p:VersionPrefix=$RELEASE_VERSION -o /app/publish

# ====== Copy to final ====== #
FROM publish AS final

RUN addgroup --system --gid 1000 netcoregroup \
&& adduser --system --uid 1000 --ingroup netcoregroup --shell /bin/sh netcoreuser

WORKDIR /app
COPY --from=publish /app/publish .

USER 1000
ENTRYPOINT ["dotnet", "Authn.dll"]